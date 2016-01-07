# ***********************************************************************
# * All sample code is provided by OSIsoft for illustrative purposes only.
# * These examples have not been thoroughly tested under all conditions.
# * OSIsoft provides no guarantee nor implies any reliability, 
# * serviceability, or function of these programs.
# * ALL PROGRAMS CONTAINED HEREIN ARE PROVIDED TO YOU "AS IS" 
# * WITHOUT ANY WARRANTIES OF ANY KIND. ALL WARRANTIES INCLUDING 
# * THE IMPLIED WARRANTIES OF NON-INFRINGEMENT, MERCHANTABILITY
# * AND FITNESS FOR A PARTICULAR PURPOSE ARE EXPRESSLY DISCLAIMED.
# ************************************************************************

param(
	[Parameter(Position=0,Mandatory=$true,ParameterSetName="reinit")]
	[switch] $Reinitialize,

	[Parameter(Position=0,Mandatory=$true,ParameterSetName="create")]
	[switch] $Create,

	[Parameter(Position=1,Mandatory=$true,ParameterSetName="reinit")]
	[Parameter(Position=1,Mandatory=$true,ParameterSetName="create")]
	[string] $PICollectiveName,
	
	[Parameter(Position=2,Mandatory=$true,ParameterSetName="create")]
	[string] $PIPrimaryName,

	[Parameter(Position=2,Mandatory=$true,ParameterSetName="reinit")]
	[Parameter(Position=3,Mandatory=$true,ParameterSetName="create")]
	[string[]] $PISecondaryNames,

	[Parameter(Position=3,Mandatory=$true,ParameterSetName="reinit")]
	[Parameter(Position=4,Mandatory=$true,ParameterSetName="create")]
	[Int32] $NumberOfArchivesToBackup,

	[Parameter(Position=4,Mandatory=$false,ParameterSetName="reinit")]
	[Parameter(Position=5,Mandatory=$false,ParameterSetName="create")]
	[switch] $ExcludeFutureArchives,
	
	[Parameter(Position=5,Mandatory=$true,ParameterSetName="reinit")]
	[Parameter(Position=6,Mandatory=$true,ParameterSetName="create")]
	[string] $BackupLocationOnPrimary
)

if ($Reinitialize -eq $true)
{
	$activity = "Reinitalizing Collective " + $connection.Name
	$serverNameUsed = $PICollectiveName
}
else
{
	$activity = "Creating Collective " + $PICollectiveName
	$serverNameUsed = $PIPrimaryName
}

$status = "Connecting to server " + $serverNameUsed
Write-Progress -Activity $activity -Status $status

$srv = Get-PIDataArchiveConnectionConfiguration -Name $serverNameUsed -ErrorAction Stop
$connection = Connect-PIDataArchive -PIDataArchiveConnectionConfiguration $srv -ErrorAction Stop

[Version] $v395 = "3.4.395"
[String] $firstPathArchiveSet1;
$includeSet1 = $false
if ($ExcludeFutureArchives -eq $false -and
    $connection.ServerVersion -gt $v395)
{
   Write-Progress -Activity $activity -Status "Getting primary archive"
   $archives = Get-PIArchiveInfo -ArchiveSet 0 -Connection $connection
   $primaryArchive = $archives.ArchiveFileInfo[0].Path

    try
    {
        $firstPathArchiveSet1 = (Get-PIArchiveInfo -ArchiveSet 1 -Connection $connection -ErrorAction SilentlyContinue).ArchiveFileInfo[0].Path
        $includeSet1 = $true
    }
    catch
    {
        $includeSet1 = $false
    }
}
else
{
   Write-Progress -Activity $activity -Status "Getting primary archive"
   $archives = Get-PIArchiveInfo -Connection $connection
   $primaryArchive = $archives.ArchiveFileInfo[0].Path
}

if ($Reinitialize -eq $true)
{
	##############################################
	# Verify secondary names specified are valid #
	##############################################

	if (($srv.Binding -is [OSIsoft.PI.Configuration.PICollectiveBinding]) -eq $false )
	{
		Write-Host "Error:" $PICollectiveName "server is not a collective."
		exit
	}

	Write-Progress -Activity $activity -Status "Verifying secondary is part of collective"
	$collectiveMembers = (Get-PICollective -Connection $connection).Members 

	foreach($secondary in $PISecondaryNames)
	{
		[bool]$found = $false
		foreach($member in $collectiveMembers)
		{
			if ($member.Role -eq "Secondary")
			{
				if ($member.Name -eq $secondary)
				{
					$found = $true
				}				
			}			
		}
		
		if ($found -eq $false)
		{
			Write-Host "Error:" $secondary "is not a secondary node of collective" $connection.Name
			exit
		}
	}	
}
else
{
	#################################
	# Verify primary name specified #
	#################################
	if ($connection -is [OSIsoft.PI.Configuration.PICollectiveBinding] -eq $true)
	{
		Write-Host "Error:" $serverNameUsed "is already a collective."
		exit
	}
	
	###########################################
	# Write collective information to primary #
	###########################################

	Write-Progress -Activity $activity -Status "Writing collective information to primary"
	New-PICollective -Name $PICollectiveName -Secondaries $PISecondaryNames -Connection $connection
}

####################################################
# Get the PI directory for each of the secondaries #
####################################################

$destinationPIPaths = @{}
foreach ($secondary in $PISecondaryNames)
{
	$session = New-PSSession -ComputerName $secondary -ErrorAction Stop -WarningAction Stop
	$destinationPIPaths.Add($secondary, (Invoke-Command -Session $session -ScriptBlock { (Get-ItemProperty (Get-Item HKLM:\Software\PISystem\PI).PSPath).InstallationPath } ))
	Remove-PSSession -Id $session.ID
}

############################
# Stop all the secondaries #
############################

foreach ($secondary in $PISecondaryNames)
{
	$status = "Stopping secondary node " + $secondary
	Write-Progress -Activity $activity -Status $status -CurrentOperation "Retrieving dependent services..."
	$pinetmgrService = Get-Service -Name "pinetmgr" -ComputerName $secondary
	$dependentServices = Get-Service -InputObject $pinetmgrService -DependentServices
	$index = 1
	foreach($dependentService in $dependentServices)
	{
		if ($dependentService.Status -ne [System.ServiceProcess.ServiceControllerStatus]::Stopped)
		{
			Write-Progress -Activity $activity -Status $status -CurrentOperation ("Stopping " + $dependentService.DisplayName) -PercentComplete (($index / ($dependentServices.Count + 1)) * 100)
			Stop-Service -InputObject $dependentService -Force -ErrorAction Stop -WarningAction SilentlyContinue
		}
		$index++
	}
	Write-Progress -Activity $activity -Status $status -CurrentOperation ("Stopping " + $pinetmgrService.Name) -PercentComplete 100
	Stop-Service -InputObject $pinetmgrService -Force -WarningAction SilentlyContinue -ErrorAction Sto
}

###########################
# Flush the archive cache #
###########################

Write-Progress -Activity $activity -Status ("Flushing archive cache on server " + $connection.Name)
Clear-PIArchiveQueue -Connection $connection

#########################
# Backup Primary Server #
#########################

$status = "Backing up PI Server " + $connection.Name
Write-Progress -Activity $activity -Status $status -CurrentOperation "Initializing..."
Start-PIBackup -Connection $connection -BackupLocation $BackupLocationOnPrimary -Exclude pimsgss, SettingsAndTimeoutParameters -ErrorAction Stop
$state = Get-PIBackupState -Connection $connection
while ($state.IsInProgress -eq $true)
{
    [int32]$pc = [int32]$state.BackupProgress.OverallPercentComplete
    Write-Progress -Activity $activity -Status $status -CurrentOperation $state.CurrentBackupProgress.CurrentFile -PercentComplete $pc
	Start-Sleep -Milliseconds 500
    $state = Get-PIBackupState -Connection $connection
}

$backupInfo = Get-PIBackupReport -Connection $connection -LastReport

###################################################
# Create restore file for each of the secondaries #
###################################################

foreach ($secondary in $PISecondaryNames)
{
	Write-Progress -Activity $activity -Status "Creating secondary restore files" -CurrentOperation $secondary
	$secondaryArchiveDirectory = Split-Path $primaryArchive
    if ($includeSet1 -eq $false)
    {
        New-PIBackupRestoreFile -Connection $connection -OutputDirectory ($BackupLocationOnPrimary + "\" + $secondary) -NumberOfArchives $NumberOfArchivesToBackup -HistoricalArchiveDirectory $secondaryArchiveDirectory
    }
    else
    {
        $secondaryArchiveSet1Directory = Split-Path $firstPathArchiveSet1
        $newArchiveDirectories = $secondaryArchiveDirectory, $secondaryArchiveSet1Directory
        New-PIBackupRestoreFile -Connection $connection -OutputDirectory ($BackupLocationOnPrimary + "\" + $secondary) -NumberOfArchives $NumberOfArchivesToBackup -ArchiveSetDirectories $newArchiveDirectories
    }
}

#################################
# Copy Backup to each secondary #
#################################

$backupLocationUNC = "\\" + $connection.Address.Host + "\" + $BackupLocationOnPrimary.SubString(0, 1) + "$" + $BackupLocationOnPrimary.Substring(2)

foreach($item in $backupInfo.Files)
{
    $totalSize += $item.Size
}

foreach($secondary in $PISecondaryNames)
{
	$destinationUNCPIRoot = "\\" + $secondary + "\" + $destinationPIPaths.$secondary.Substring(0, 1) + "$" + $destinationPIPaths.$secondary.Substring(2)

	$status = "Copying backup to secondary node"
	$currentSize = 0
	foreach($file in $backupInfo.Files)
	{
        $currentSize += $file.Size
		Write-Progress -Activity $activity -Status $status -CurrentOperation $file.Name -PercentComplete (($currentSize / $totalSize) * 100)
		$sourceUNCFile = "\\" + $connection.Address.Host + "\" + $file.Destination.SubString(0, 1) + "$" + $file.Destination.Substring(2)
		if ($file.ComponentDescription.StartsWith("Archive") -eq $true)
		{
            $destinationFilePath = Split-Path $file.Destination
            if ($destinationFilePath.EndsWith("arcFuture") -eq $true)
            {
                $destinationUNCPath = $destinationUNCPIRoot + $secondaryArchiveSet1Directory.Replace($destinationPIPaths.$secondary, "")
            }
            else
            {
			    $destinationUNCPath = $destinationUNCPIRoot + $secondaryArchiveDirectory.Replace($destinationPIPaths.$secondary, "")
            }
		}
		else
		{
			$destinationUNCPath = $destinationUNCPIRoot + (Split-Path $file.Destination).Replace($BackupLocationOnPrimary, "")
		}
		
		Copy-Item -Path $sourceUNCFile -Destination $destinationUNCPath

		$index++
	}

	$piarstatUNC = $backupLocationUNC + "\" + $secondary
	Copy-Item -Path ($piarstatUNC + "\piarstat.dat") -Destination ($destinationUNCPIRoot + "\dat")
	# We only need this file for one server, it's ok to delete it now
	Remove-Item -Path ($piarstatUNC + "\piarstat.dat")
}

########################
# Cleanup backup files #
########################

foreach ($file in $backupInfo.Files)
{
	$sourceUNCFile = "\\" + $connection.Address.Host + "\" + $file.Destination.SubString(0, 1) + "$" + $file.Destination.Substring(2)
	Remove-Item -Path $sourceUNCFile	
}

[Int32]$count = (Get-ChildItem $backupLocationUNC -Recurse | where {$_.psIsContainer -eq $false}).Count

if ($count -eq 0)
{
	Write-Progress -Activity $activity -Status "Removing empty backup directories."
	Remove-Item -Path $backupLocationUNC -Recurse
}

#########################
# Start all secondaries #
#########################

[string[]] $piServices = "pinetmgr", "pimsgss", "pilicmgr", "piupdmgr", "pibasess", "pisnapss", "piarchss", "pibackup"
foreach($secondary in $PISecondaryNames)
{
	foreach($service in $piServices)
	{
		$service = Get-Service -ComputerName $secondary -Name $service
		Write-Progress -Activity $activity -Status ("Starting secondary node " + $secondary) -CurrentOperation ("Starting " + $service.DisplayName)
		Start-Service -InputObject $service -WarningAction SilentlyContinue
	}
}
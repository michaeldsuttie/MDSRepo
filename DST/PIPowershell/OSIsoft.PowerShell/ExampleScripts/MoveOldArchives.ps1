# ***********************************************************************
# * DISCLAIMER: 
# * All sample code is provided by OSIsoft for illustrative purposes only.
# * These examples have not been thoroughly tested under all conditions.
# * OSIsoft provides no guarantee nor implies any reliability, 
# * serviceability, or function of these programs.
# * ALL PROGRAMS CONTAINED HEREIN ARE PROVIDED TO YOU "AS IS" 
# * WITHOUT ANY WARRANTIES OF ANY KIND. ALL WARRANTIES INCLUDING 
# * THE IMPLIED WARRANTIES OF NON-INFRINGEMENT, MERCHANTABILITY
# * AND FITNESS FOR A PARTICULAR PURPOSE ARE EXPRESSLY DISCLAIMED.
# ************************************************************************

# *****************************************************************************************************
# **** NOTE: PSRemoting must be enabled on both client and server machines (via Enable-PSRemoting) ****
# *****************************************************************************************************
param(
	[Parameter(Position=0, Mandatory=$true)]
	[string] $PIServerName,

	[Parameter(Position=1, Mandatory=$true)]
	[DateTime] $MoveArchivesOlderThan,

	[Parameter(Position=2, Mandatory=$true)]
	[string] $MoveToLocation,

	[Parameter(Position=3, Mandatory=$false)]
	[switch] $MakeReadOnly = $false)

$MoveItem = {
    param([string] $Name, 
          [string] $MoveToLocation,
          [string] $NewArchiveFullName,
          [bool]   $MakeReadOnly)
           
    # If the path specified doesn't exist
    if ((Test-Path $MoveToLocation) -eq $false)
    {
        # Create the directory, stop and exit if it fails
        $NewDir = New-Item $MoveToLocation -type directory -ErrorAction stop
    }
    
    # Move the archive to the new location
    Move-Item $Name $MoveToLocation
    # Check to see if there is an annotation file
    if ((Test-Path ($Name + ".ann")) -eq $true)
    {
        Move-Item ($Name + ".ann") $MoveToLocation
    }
    
    # Check to see if we are marking the archive file as readonly
    if ($MakeReadOnly -eq $true)
    {
        # Set the file to read only
        Set-ItemProperty $NewArchiveFullName -name IsReadOnly -value $true
    }    
}

# Get the PI server object, exit if there is an error retrieving the server
$srv = Get-PIDataArchiveConnectionConfiguration -Name $PIServerName -ErrorAction Stop
$connection = Connect-PIDataArchive -PIDataArchiveConnectionConfiguration $srv -ErrorAction Stop

[Version] $v395 = "3.4.395"
if ($connection.ServerVersion -gt $v395)
{
   $archives = Get-PIArchiveFileInfo -Connection $connection -ArchiveSet 0 -ErrorAction Stop
}
else
{
   $archives = Get-PIArchiveFileInfo -Connection $connection -ErrorAction Stop
}

# Iterate through each archive and test to see if the archive should be moved
$archives | ForEach-Object {
   if ($_.Index -ne 0 -and                      		  # Make sure that we aren't looking at the primary archive
       $_.EndTime -ne $null -and                        # Verify that the archive has a start time
       $_.EndTime -ne [System.DateTime]::MinValue -and  # Verify that the archive start time is set
       $_.EndTime -lt $MoveArchivesOlderThan)           # Check to see if the archive start time is older than the time specified
   {
      Write-Host "Unregistering archive" $_.Path
      # Unregister the archive
      Unregister-PIArchive -Name $_.Path -Connection $connection -ErrorAction continue
           
      # Get the new path and file name
      $NewArchiveFullName = [System.IO.Path]::Combine($MoveToLocation, [System.IO.Path]::GetFileName($_.Path))
      
      Write-Host "Moving archive" $_.Path "to" $NewArchiveFullName
      # Use Invoke-Command to move the archive files on the PI server.  
      $hasError = $null
      Invoke-Command -computername $PIServerName -scriptblock $MoveItem -ArgumentList $_.Path, $MoveToLocation, $NewArchiveFullName, $MakeReadOnly -ErrorAction Inquire -ErrorVariable hasError
        
      if ($hasError -eq $null -or
          $hasError.Count -eq 0)
      {
		   Write-Host "Registering archive" $NewArchiveFullName
	      # Register the archive
	      Register-PIArchive -Name $NewArchiveFullName -Connection $connection 
      }
      else
      {
         Write-Host "Skip registering archive" $_.Path "due to error during move."
      }
   }
   else
   {
      Write-Host "Archive" $_.Path "is not old enough to move."
   }
}
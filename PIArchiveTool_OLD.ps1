#Adds required Powershell Library
if ((Get-PSSnapin "OSIsoft.PowerShell" -ErrorAction SilentlyContinue) -eq $null) { Add-PSSnapin "OSIsoft.PowerShell" }

#Adds required Visual Studio assembly
[void][System.Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')

#################INITIALIZATION_BLOCK#################
$PIServerName = [system.net.dns]::GetHostName()
$PI = $null
[Array]$Archives = $null
$Archive = $null
$CreateNew = 'y'
$ArchiveDirectory = 'C:\Program Files\PI\dat\'
$ArchiveStartDate = 'DD-MMM-YYYY'
$ArchiveEndDate = $null
$ArchiveDateRange = $null
$ArchiveStartTime = $null
$ArchiveEndTime = $null
$Exit = 'n'
######################################################

$PIServerName = [Microsoft.VisualBasic.Interaction]::InputBox('Enter PI Server Name','PI Server Name',$PIServerName)

#Connects to PI Server. If server connection already exists, it silently continues.
[OSIsoft.SMT.PIServer]$PI = Get-PIServer -Name $PIServerName -ErrorAction Stop | Connect-PIServer -ConnectionPreference RequirePrimary -ErrorAction SilentlyContinue

#Checks PI Server Connection
if ($PI -eq $null) 
{
    Write-Host "Uh oh, no connection the PI Server." -ForegroundColor Red
}


#Retrieves PI Archives & Opens New Grid-Box
[Array]$Archives = Get-PIArchive -PIServer $PI | where {$_.StartTime -ne $null} | sort -Property StartTime -Descending | Out-GridView -Title "PI Archives on $PIServerName"

#Prompts user to Create New Archive
$CreateNew = [Microsoft.VisualBasic.Interaction]::InputBox('Create New Archive (y/n)','Create New Archive',$CreateNew)

while($CreateNew -eq 'y')
{
    $ArchiveDirectory = [Microsoft.VisualBasic.Interaction]::InputBox('Enter PI Archive Directory','Archive Directory',$ArchiveDirectory)
    Write-Host $ArchiveDirectory

    Do
    {
        $ArchiveStartDate = [Microsoft.VisualBasic.Interaction]::InputBox('Enter PI Archive Start Date (DD-MMM-YYYY)','Archive Start Date',$ArchiveStartDate)
        Write-Host $ArchiveStartDate
    }
    while($ArchiveStartDate -eq 'DD-MMM-YYYY')

    $ArchiveEndDate = $ArchiveStartDate
    
    Do
    {
        $ArchiveEndDate = [Microsoft.VisualBasic.Interaction]::InputBox('Enter PI Archive End Date (DD-MMM-YYYY)','Archive End Date',$ArchiveEndDate) -f $Archive.EndDate
        Write-Host $ArchiveEndDate
    }
    While($ArchiveEndDate -eq $ArchiveStartDate)

    $ArchiveDateRange = $ArchiveStartDate + '_' + $ArchiveEndDate
    Write-Host $ArchiveDateRange

    $ArchivePath = $ArchiveDirectory + $PIServerName +'_' + $ArchiveDateRange + '.arc'
    #$ArchivePath = [String]::concat($ArchiveDirectory, $PIServerName, '_', $ArchiveDateRange, '.arc')
    Write-Host $ArchivePath

    $ArchiveStartTime  = $ArchiveStartDate + ' 00:00:00'
    Write-Host $ArchiveStartTime

    $ArchiveEndTime = $ArchiveEndDate + ' 00:00:00'
    Write-Host $ArchiveEndTime

    & 'C:\Program Files\PI\adm\piartool.exe' -acd $ArchivePath $ArchiveStartTime $ArchiveEndTime
    
    $ArchiveStartDate = $ArchiveEndDate

    [Array]$Archives = Get-PIArchive -PIServer $PI | where {$_.StartTime -ne $null} | sort -Property StartTime -Descending | Out-GridView -Title "PI Archives on $PIServerName"

    $CreateNew = [Microsoft.VisualBasic.Interaction]::InputBox('Create New Archive (y/n)','Create New Archive',$CreateNew)
}

#Prompts User To Continue
$Exit = [Microsoft.VisualBasic.Interaction]::InputBox('Process Complete. Create Another Archive? (y/n)','Exit Message',$Exit)
if($Exit -eq 'y' -or 'Y')
{
    exit
}


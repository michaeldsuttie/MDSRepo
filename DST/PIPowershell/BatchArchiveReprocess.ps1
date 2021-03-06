#param(
#    [Parameter(Position=0, Mandatory=$true)]
#    [string] $PIServerName)

if ((Get-PSSnapin -Name OSIsoft.PowerShell -ErrorAction SilentlyContinue) -eq $null)
{
    Add-PSSnapin OSIsoft.PowerShell
}
 

# Get PI Server name.  
$PIServerName = "morp12wsapx01"  
#$PIServerName = Read-Host "Enter PI Server Name"
 
# Get Archive Index Range.
$ArchiveIndexFirst = Read-Host "Enter Archive Index of First Archive to Process"
$ArchiveIndexLast = Read-Host "Enter Archive Index of Last Archive to Process"
#$ArchiveIndexFirst = 283
#$ArchiveIndexFirst = 283


# Get the PI server object, exit if there is an error retrieving the server
$PIServer = Get-PIServer -Name $PIServerName -ErrorAction Stop

# Get the collection of all archives
$ArchiveList = [OSIsoft.SMT.PIArchive]::Get($PIServer)
 
# Create a string template to output
$StringTemplate = [string]"Archive[{0}]: {1}" + "`r`n" `
                        + "Is Primary: {2}" + "`r`n" `
                        + "Is Reprocessed: {3}" + "`r`n" `
                        + "Is Selected: {4}" + "`r`n" `
                        + "Start Time: {5}" + "`r`n" `
                        + "End Time: {6}" + "`r`n" `
                        + "Unregister Command: {7}" + "`r`n" `
                        + "Reprocess Command: {8}" + "`r`n" `
                        + "Register Command: {9}" 
                        
$OutputString = ""
                 
# Browse to extract them one by one.
foreach($ArchiveItem in $ArchiveList)
{               
    # Create the IsPrimary flag.
    if($ArchiveItem.Index -eq 0) { $IsPrimary = 1 }
    else { $IsPrimary = 0 }
    
    # Get the Archive Filename.
    $ArchiveFileName = $ArchiveItem.Name.ToString()
    
    # Create IsReprocessed flag. 
    if($ArchiveFileName -match "reprocessed") { $IsReprocessed = 1 }
    else { $IsReprocessed = 0 }
    
    # Create IsSelected Flag.
    if(($ArchiveItem.Index -ge $ArchiveIndexFirst) -and ($ArchiveItem.Index -le $ArchiveIndexLast)) {$IsSelected = 1}
    else { $IsSelected = 0 }
     
    # Check if the Start Time is null.
    if($ArchiveItem.StartTime -eq $null) { $StartTimeInString = "Current Time" }
    else{ $StartTimeInString = $ArchiveItem.StartTime.ToString("yyyy-MM-dd_HH-mm-ss") }      
      
    # Check if the End Time is null.
    if($ArchiveItem.EndTime -eq $null) { $EndTimeInString = "Current Time" }    
    else{ $EndTimeInString = $ArchiveItem.EndTime.ToString("dd-MMM-yyyy HH:mm:ss.fffff") }   
    
    # Build Standardized Name. Based on Start Time.
    #$ArchiveStandardizedName = $ArchiveFileName.Substring(0,23) + "_" + $StartTimeInString + ".arc"
    $ArchiveStandardizedName = "D:\PI\arc\MORP12WSAPX01_" + $StartTimeInString + ".arc"
    
    # Build Commands.
    $UnregisterCommand = "`"`"%piserver%\adm\piartool.exe`" -au " + $ArchiveFileName + "`""
    $ReprocessCommand = "`"`"%piserver%\bin\piarchss.exe`" -if " + $ArchiveFileName  + " -of " + $ArchiveStandardizedName + ".reprocessed" + "`""       
    $RegisterCommand = "`"`"%piserver%\adm\piartool.exe`" -ar " + $ArchiveStandardizedName + ".reprocessed" + "`"" 
    #$MoveCommand = Move-Item 

    # Build the output for a given archive.
    $OutputString = [string]::Format($StringTemplate, `
                                    [Convert]::ToString($ArchiveItem.Index), `
                                    $ArchiveFileName, `
                                    $IsPrimary.ToString(), `
                                    $IsReprocessed.ToString(), `
                                    $IsSelected.ToString(), `
                                    $StartTimeInString, `
                                    $EndTimeInString, `
                                    $UnregisterCommand,`
                                    $ReprocessCommand, `
                                    $RegisterCommand)   
                                                                     
                                    
    if(($IsPrimary -eq 0) -and ($IsSelected -eq 1))
    {
        Write-Host $OutputString
        CMD /C $UnregisterCommand
        CMD /C $ReprocessCommand
        CMD /C $RegisterCommand
        Write-Host "----------"
    }
}
  
Write-Host "End of listing."
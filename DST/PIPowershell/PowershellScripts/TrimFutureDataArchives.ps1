
$numOfDaysToKeep = 60
$currentDate = Get-Date
$PIDataArchiveName = "MyPIDataArchive"
$TargetFolder = "c:\MyOldArchives"

#Set this to 0 for historical archives, 1 for future archives
$ArchiveSet = 1

#Switch to determine if archives should be moved and re-registered or just moved (to ultimately be deleted)
#0 = move and reregister
#1 = just move
$MigrateOrRemove = 1

#Put a subfolder underneath for obsolete archives
if ($MigrateOrRemove -eq 1)
{$TargetFolder = $TargetFolder + "\ObsoleteArchives"}

#Check to see if TargetFolder exists...if not, create it:

If(!(Test-Path -Path $TargetFolder))
{
    New-Item -ItemType directory -Path $TargetFolder
}

$PIDataArchive = Connect-PIDataArchive -PIDataArchiveMachineName $PIDataArchiveName



#Get the list of all PI Archives on the connected $PIDataArchive
$PIArchives = Get-PIArchiveFileInfo -Connection $PIDataArchive -ArchiveSet $ArchiveSet

#Go through each Archive in the list, first check to see if it's already in the Target folder, 
# and then check to see if its end time is before the day cutoff

foreach ($PIArchive in $PIArchives)
{
   $PIArchiveFolder = Split-Path -Path $PIArchive.Path -Parent
  if($PIArchiveFolder -ne $TargetFolder)
   {
   
   
    if ($PIArchive.EndTime)
    {
     $daysOld = ($currentDate - $PIArchive.EndTime).Days
     if ( $daysOld -ge $numOfDaysToKeep)
        {
            
            #Grab the file name
            
            $ArchiveName = Split-Path -Path $PIArchive.Path -Leaf
            
                      
            
            #unregister Archive
            Unregister-PIArchive -Name $PIArchive.Path -Connection $PIDataArchive
            
            #Pause to make sure the archive is fully unregistered before trying to move
            Start-Sleep -m 2500
            #Move Archive and .ann file
            
            Move-Item -Path $PIArchive.Path -Destination $TargetFolder
            Move-Item -Path ($PIArchive.Path+".ann") -Destination $TargetFolder
            #Build the full path name
            $NewArchivePath = ($TargetFolder + "\" + $ArchiveName)
            

            if ($MigrateOrRemove -eq 0) 
            {
            #Pause to make sure file is copied over first
            Start-Sleep -m 2500
            #Finally register moved archive
            Register-PIArchive -Connection $PIDataArchive -Name $NewArchivePath
            }
        }
     
     
     } 
   }
}
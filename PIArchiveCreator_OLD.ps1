#Loads Visual Studio I\O Dialog Boxes
[void][System.Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')

$archiveDirectory = [Microsoft.VisualBasic.Interaction]::InputBox('Enter PI Archive Directory','Archive Directory','C:\Program Files\PI\dat\')
Write-Host $archiveDirectory

##########HARD_CODE###############
$archiveStartDate = '2013-01-01'
$archiveEndDate = '2013-02-01'
##################################

#$archiveStartTime = [Microsoft.VisualBasic.Interaction]::InputBox('Enter PI Archive Start Time (YYYY-MM-DD)','Archive Start Time','YYYY-MM-DD')
#Write-Host $archiveStartTime

#$archiveEndTime = [Microsoft.VisualBasic.Interaction]::InputBox('Enter PI Archive End Time (YYYY-MM-DD)','Archive End Time',$archiveStartTime)
#Write-Host $archiveEndTime

$archiveDateRange = $archiveStartDate + '_' + $archiveEndDate
Write-Host $archiveDateRange

$computerName = [system.net.dns]::GetHostName()

$archivePath = $archiveDirectory + $computerName +'_' + $archiveDateRange + '.arc'
Write-Host $archivePath

$PIArToolDirectory = cd $archiveDirectory\..\adm\
Write-Host $PIArToolDirectory

$archiveStartTime = "01-JAN-2013 00:00:00"
$archiveEndTime = "01-FEB-2013 00:00:00"

& 'C:\Program Files\PI\adm\piartool.exe' -acd $archivePath $archiveStartTime $archiveEndTime

 #JUNK BELOW
 #$archiveStartTime = [Microsoft.VisualBasic.Interaction]::InputBox('Enter PI Archive Start Time (YYYY-MM-DD)','Archive Start Time','YYYY-MM-DD')
 #Write-Host $archiveStartTime

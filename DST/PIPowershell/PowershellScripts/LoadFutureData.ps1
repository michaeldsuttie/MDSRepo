
#Parameters

$PIDataArchiveName = "MyPIDataArchive"

#Number of days to go back and get data
$DaysOfData = 14
#Number of days to push this forward
$TimeOffset = 7
#Percent increase of value
$DataModification = 15
#List of future/historical Points:  make sure they match from previous script
$PIFuturePointPath = "c:\exportFolder\FuturePointList.txt"
$PIOriginalPointPath = "c:\exportFolder\OriginalPointList.txt"

#Specify some Times
$currTime = Get-Date
$startTime = $currTime.AddDays(-1*$DaysOfData)
#$shiftTime = $currTime.AddDays(-1*($DaysOfData - $TimeOffset))

#Connect to PI 
$PIDataArchive = Connect-PIDataArchive -PIDataArchiveMachineName $PIDataArchiveName
#Get list of PI Points

[System.Array] $PIOriginalPointList = Get-Content $PIOriginalPointPath
[System.Array] $PIFuturePointList = Get-Content $PIFuturePointPath


for ($i=0; $i -lt $PIOriginalPointList.Length; $i++)
 {
    Write-Host ("Working on Point " + $PIOriginalPointList[$i] + " to future point " + $PIFuturePointList[$i] ) 
   $PIPoint = Get-PIPoint -Connection $PIDataArchive -Name $PIOriginalPointList[$i] -ErrorAction Stop
    $PIPoint_Future = Get-PIPoint -Connection $PIDataArchive -Name $PIFuturePointList[$i] -ErrorAction SilentlyContinue
    if (!$PIPoint_Future)
    {
        Write-Host "No future point created yet, run CreateFutureDataTags.ps1 first"
        exit
    }
    
    $PIPointValues = Get-PIValue -PIPoint $PIPoint -StartTime $startTime -EndTime $currTime
    
    #Initialize empty arrays for the values and timestamps
    $PIFutureValuesArray = @()
    [DateTime[]] $PIFutureTimesArray = @()
    for($j=0; $j -lt $PIPointValues.Count; $j++)
    {
        #Check to make sure the value is a value type (i.e. not a string or digital state), else
        #convert it to our favority irrational number
        $PIFutureValue = 3.14159265
        if ($PIPointValues[$j].Value.GetType().Name -ne "EventState" -and $PIPointValues[$j].Value.GetType().BaseType.Name -eq "ValueType")
        {$PIFutureValue = $PIPointValues[$j].Value * (1+$DataModification/100)}
        else {$PIFutureValue = 3.14159265}
        $PIFutureValuesArray = $PIFutureValuesArray + $PIFutureValue
        $PIFutureTime = $PIPointValues[$j].Timestamp.AddDays($TimeOffset)
        $PIFutureTimesArray = $PIFutureTimesArray + $PIFutureTime
        
    }
    $FutureDataAdded = Add-PIValue -Pointname $PIFuturePointList[$i] -Connection $PIDataArchive  -Value $PIFutureValuesArray -Time $PIFutureTimesArray -WriteMode Replace
 

}
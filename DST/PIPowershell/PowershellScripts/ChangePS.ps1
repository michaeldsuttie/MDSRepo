$PIDataArchiveName = "MyPIDataArchive"

$PointPrefix = "Intense"
$PointSourceOld = "R"
$PointSourceNew = "XXX"

$PointAttr = @{PointSource=$PointSourceNew}

$PIDataArchive = Connect-PIDataArchive -PIDataArchiveMachineName $PIDataArchiveName

$WhereClause = $whereClause = ("pointsource:=" + $PointSourceOld + " Tag:="+ $PointPrefix + "*" )

$PIPointsToModify = Get-PIPoint -Connection $PIDataArchive -WhereClause $WhereClause

foreach($PIPoint in $PIPointsToModify)
{
Write-Host ("Modifying Point: " + $PIPoint.Point.Name)
Set-PIPoint -Connection $PIDataArchive -Name $PIPoint.Point.Name -Attributes $PointAttr
}
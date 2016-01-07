

$PIDataArchiveName = "MyPIDataArchive"

#Once the Points are created, we will write a list to the following file:
$PointListFolder = "c:\exportFolder\"
$FuturePointListPath = ($PointListFolder + "FuturePointList.txt")
$OriginalPointListPath = ($PointListFolder + "OriginalPointList.txt")
If(!(Test-Path -Path $PointListFolder))
{
    New-Item -ItemType directory -Path $PointListFolder
}


#Connect to PI and make sure it's running a future-data compatible version


$PIDataArchive = Connect-PIDataArchive -PIDataArchiveMachineName $PIDataArchiveName

if ($PIDataArchive.ServerVersion.Build -lt 395)
{write-Host "Using a non-future data aware version of PI...update to 395!"
exit}


#Get list of all PI points from Random interface

$whereClause = "pointsource:=R"
$PIRandomPoints = Get-PIPoint -Connection $PIDataArchive -WhereClause $whereClause

$futurePointSuffix = "_MySuffix"

$newFuturePointAttr =  @{
                    "future" = "1"
                    "pointsource" = "FutureLAB"
                    }

#Open two text stream writers
$streamWrite = [System.IO.StreamWriter] $FuturePointListPath
$streamWrite2 = [System.IO.StreamWriter] $OriginalPointListPath


foreach ($PIPoint in $PIRandomPoints)
{
#Write the original PI point to a list
$streamWrite2.WriteLine($PIPoint.Point.Name)

#Then check to see if future point exists, if not create it
 $futurePIPoint = Get-PIPoint -Name ($PIPoint.Point.Name+ $futurePointSuffix) -connection $PIDataArchive -ErrorAction SilentlyContinue
        if (!$futurePIPoint)
        {
         #Build attr hashtable
         $newFuturePointAttr =  @{
                    "future" = "1"
                    "pointsource" = "FutureLAB"
                    "pointclass" = "Classic"
                    "pointtype" = $PIPoint.Point.Type
                    }
         #Create new point
         Write-Host ("Creating New point:  " + $PIPoint.Point.Name + $futurePointSuffix)
         $futurePIPoint = Add-PIPoint -Name ($PIPoint.Point.Name+ $futurePointSuffix) -Connection $PIDataArchive `
                             -Attributes $newFuturePointAttr
                            
        }
         $streamWrite.WriteLine($futurePIPoint.Point.Name)

}


#Close text file stream writing
$streamWrite.Close()
$streamWrite2.Close()
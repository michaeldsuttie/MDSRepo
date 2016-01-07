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
param (
    [Parameter(Position=0, Mandatory=$true)]
    [string]$PIServerName,
    
    [Parameter(Position=1,Mandatory=$false)]
    [int]$Wait = 3
)

# Get the PI Server object
$PIServer = Get-PIDataArchiveConnectionConfiguration $PIServerName -ErrorAction Stop

$PIServerConnection = Connect-PIDataArchive -PIDataArchiveConnectionConfiguration $PIServer -ErrorAction Stop

# Get an initial start time and end time, we'll show the last minutes worth of messages to begin with
[DateTime]$st = (Get-Date).AddMinutes(-1)
[DateTime]$et = Get-Date

do
{   
	# Retrieve messages from the server.  If there are no messages, Get-PIMessage will return an
	# error, so we will silently ignore that.
	Get-PIMessage -StartTime $st -EndTime $et -Connection $PIServerConnection -ErrorAction SilentlyContinue | ForEach-Object {
		# To format this the same as pigetmsg, we first just want the first character of the severity
		$sev = $_.Severity.ToString().SubString(0,1)
		
		# Set the color of the message based on its severity
		switch ($_.Severity)
		{
			Critical { $newForegroundColor = "Red" }
			Error { $newForegroundColor = "Red" }
			Warning { $newForegroundColor = "White" }
			Informational { $newForegroundColor = "Gray" }
			Debug { $newForegroundColor = "DarkGray" }
			default { $newForegroundColor = -1 }
		}

		# If Source1 is empty, we want to put a colon after the Program name to display Source1
		if ([string]::IsNullOrEmpty($_.Source1) -eq $true)
		{
			# To right justify the ID, get the size of the screen, and subtract the length of
			# the items displayed
			$width = $Host.UI.RawUI.WindowSize.Width - (27 + $_.ProgramName.Length + $_.ID.ToString().Length)
            if ($newForegroundColor -eq -1)
            {
    			Write-Host ("{0} {1:dd-MMM-yyyy HH:mm:ss} {2} {4,$width}({3})" -f $sev, $_.TimeStamp, $_.ProgramName, $_.ID, "")
            }
            else
            {
    			Write-Host ("{0} {1:dd-MMM-yyyy HH:mm:ss} {2} {4,$width}({3})" -f $sev, $_.TimeStamp, $_.ProgramName, $_.ID, "") -ForegroundColor $newForegroundColor
            }
		}
		else
		{
			$width = $Host.UI.RawUI.WindowSize.Width - (28 + $_.ProgramName.Length + $_.Source1.Length + $_.ID.ToString().Length)
            if ($newForegroundColor -eq -1)
            {
			    Write-Host ("{0} {1:dd-MMM-yyyy HH:mm:ss} {2}:{3} {5,$width}({4})" -f $sev, $_.TimeStamp, $_.ProgramName, $_.Source1, $_.ID, "")
            }
            else
            {
			    Write-Host ("{0} {1:dd-MMM-yyyy HH:mm:ss} {2}:{3} {5,$width}({4})" -f $sev, $_.TimeStamp, $_.ProgramName, $_.Source1, $_.ID, "") -ForegroundColor $newForegroundColor
            }
		}

        if ($newForegroundColor -eq -1)
        {
    		Write-Host (" >> {0}" -f $_.Message)
        }
        else
        {
    		Write-Host (" >> {0}" -f $_.Message) -ForegroundColor $newForegroundColor
        }
	    Write-Host
	}

	# Wait for the specified seconds
    Start-Sleep -seconds $Wait
	
	# Update the start time to be just after the current end time
	$st = $et.AddMilliseconds(1)
	# Update the end time to be now
	$et = Get-Date
} while($true)
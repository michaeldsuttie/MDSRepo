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

param(
	[Parameter(Position=0, Mandatory=$true)]
	[string] $PIServerName,
	
	[Parameter(Position=1, Mandatory=$false)]
	[DateTime] $StartTime,
	
	[Parameter(Position=2, Mandatory=$false)]
	[DateTime] $EndTime)

$srv = Get-PIDataArchiveConnectionConfiguration -Name $PIServerName -ErrorAction Stop
$connection = Connect-PIDataArchive -PIDataArchiveConnectionConfiguration $srv -ErrorAction Stop

[Version] $v390 = "3.4.390"
[Version] $v385 = "3.4.385"

[bool] $is390 = $false
[bool] $is385 = $false

if ($connection.ServerVersion -gt $v390)
{
	$is390 = $true
}
elseif ($connection.ServerVersion -gt $v385)
{
	$is385 = $true
}
else
{
	"Unsupported PI server version found."
	exit
}

# If StartTime is not passed in, get the startup time of pinetmgr and use that as the StartTime
if ($StartTime -eq $null)
{
	$service = Get-WmiObject win32_service -filter "name = 'pinetmgr'" -ComputerName $connection.Address.Host
	$serverStartup = ((Get-Date) - ([wmi]'').ConvertToDateTime((Get-WmiObject Win32_Process -ComputerName $connection.Address.Host -filter "ProcessID = '$($service.ProcessId)'").CreationDate))
	
	$StartTime = (Get-Date) - $serverStartup
}

# If EndTime is not passed in, use current time as end time
if ($EndTime -eq $null)
{
	$EndTime = Get-Date
}

# Get all the connections since StartTime
# Message ID's are the following:
# 7039 - Begin connection
# 7080 - Connection information
# 7096 - End connection
# 7121 - End connection
# 7133 - Connection Statistics
$messages = Get-PIMessage -Connection $connection -StartTime $StartTime -EndTime $EndTime -ID 7039,7080,7096,7121,7133

# Store all the active connection information in a hashtable of obects.
# The hashtable is indexed by Connection ID
# When a connection is completed, move the entry from the Hashtable into an array
# This is to handle reused Connection IDs
[Hashtable] $activeConnections = @{}
[Array] $closedConnections = @()

foreach($item in $messages)
{
	if ($item.ID -eq 7039)
	{
		# begin connection message
		if ($item.Message -match "Process name:\s*(.*) ID: (.*)" -eq $true)
		{
			# $Matches[1] Process Name
			# $Matches[2] Connection ID
			
			$id = $Matches[2].Trim() -as [Int32]
			if ($id -ne $null -and $activeConnections.ContainsKey($id) -eq $false)
			{
				#Parse out connection information
				$appInfo = $Matches[1]
				if ($appInfo -match "(.*)\((.*)\):(.*)\((.*)\)" -eq $true)
				{
					$isRemote = $true
					$appName = $Matches[1]
					$appPID = $Matches[2]
				}
				elseif ($appInfo -match "(.*)\((.*)\)" -eq $true)
				{
					$isRemote = $false
					$appName = $Matches[1]
					$appPID = $Matches[2]
				}
				else
				{
					$isRemote = $null
					$appName = $appInfo
					$appPID = $null
				}
				
				$temp = New-Object PSCustomObject
				Add-Member -InputObject $temp -MemberType NoteProperty -Name "ID" -Value $id
				Add-Member -InputObject $temp -MemberType NoteProperty -Name "ApplicationName" -Value $appName
				Add-Member -InputObject $temp -MemberType NoteProperty -Name "ApplicationPID" -Value $appPID
				Add-Member -InputObject $temp -MemberType NoteProperty -Name "IsRemote" -Value $isRemote
				Add-Member -InputObject $temp -MemberType NoteProperty -Name "PIUser" -Value $null
				Add-Member -InputObject $temp -MemberType NoteProperty -Name "OSUser" -Value $null
				Add-Member -InputObject $temp -MemberType NoteProperty -Name "IPAddress" -Value $null
				Add-Member -InputObject $temp -MemberType NoteProperty -Name "Duration" -Value $null
				Add-Member -InputObject $temp -MemberType NoteProperty -Name "StartTime" -Value $item.LogTime
				Add-Member -InputObject $temp -MemberType NoteProperty -Name "EndTime" -Value $null
				Add-Member -InputObject $temp -MemberType NoteProperty -Name "KBSent" -Value $null
				Add-Member -InputObject $temp -MemberType NoteProperty -Name "KBReceived" -Value $null
				Add-Member -InputObject $temp -MemberType NoteProperty -Name "DisconnectReason" -Value $null
				$activeConnections.Add($id, $temp)
			}
		}
	}
	elseif ($item.ID -eq 7080)
	{
		# connection information message 3.4.390
		if ($is390 -eq $true)
		{
			if ($item.Message -match "Connection ID: (.*) ; Process name: (.*) ; User: (.*) ; OS User: (.*) ; Hostname: (.*) IP: (.*) ; AppID: (.*) ; AppName: (.*)" -eq $true)
			{
				# $Matches[1] Connection ID
				# $Matches[3] PIUser
				# $Matches[4] OSUser
				# $Matches[6] IP address
				
				$id = $Matches[1].Trim() -as [Int32]
				if ($id -ne $null -and $activeConnections.ContainsKey($id) -eq $true)
				{
					$activeConnections[$id].PIUser = $Matches[3].Trim()
					$activeConnections[$id].OSUser = $Matches[4].Trim()
					$activeConnections[$id].IPAddress = $Matches[6].Trim()
				}
			}
		}
		# connection information message 3.4.385
		elseif ($is385 -eq $true)
		{
			if ($item.Message -match "Connection ID: (.*) ; Process name: (.*) ; User: (.*) ; OS User: (.*) ; IP: (.*) ; AppID: (.*) ; AppName: (.*)" -eq $true)
			{
				# $Matches[1] Connection ID
				# $Matches[3] PIUser
				# $Matches[4] OSUser
				# $Matches[5] IP address
				
				$id = $Matches[1].Trim() -as [Int32]
				if ($id -ne $null -and $activeConnections.ContainsKey($id) -eq $true)
				{
					$activeConnections[$id].PIUser = $Matches[3].Trim()
					$activeConnections[$id].OSUser = $Matches[4].Trim()
					$activeConnections[$id].IPAddress = $Matches[5].Trim()
				}
			}
		}
	}
	elseif ($item.ID -eq 7096 -or $item.ID -eq 7121)
	{
		#end connection message
		if ($item.Message -match "Deleting connection: (.*), (.*), ID: (.*) (.*)" -eq $true)
		{
			# $Matches[1] Application name
			# $Matches[2] Disconnect reason
			# $Matches[3] Connection ID
			# $Matches[4] Connection address
			
			$id = $Matches[3].Trim() -as [Int32]
			if ($id -ne $null -and $activeConnections.ContainsKey($id) -eq $true)
			{
				$activeConnections[$id].DisconnectReason = $Matches[2].Trim()
				$activeConnections[$id].EndTime = $item.LogTime
				if ($activeConnections[$id].StartTime -ne $null)
				{
					$activeConnections[$id].Duration = $activeConnections[$id].EndTime - $activeConnections[$id].StartTime
				}
			}
		}
	}
	elseif ($item.ID -eq 7133)
	{
		#Connection Statistics message
		if ($item.Message -match "ID: (.*); Duration: (.*); kbytes sent: (.*); kbytes recv: (.*); app: (.*); user: (.*); osuser: (.*); trust: (.*); ip address: (.*); ip host: (.*)" -eq $true)
		{
			# $Matches[1] Connection ID
			# $Matches[3] KBSent
			# $Matches[4] KBReceived
			
			$id = $Matches[1].Trim() -as [Int32]
			if ($id -ne $null -and $activeConnections.ContainsKey($id) -eq $true)
			{
				$activeConnections[$id].KBSent = $Matches[3] -as [Float]
				$activeConnections[$id].KBReceived = $Matches[4] -as [Float]
				
				# Copy connection information into closed connections array
				$closedConnections += $activeConnections[$id]
				# Remove active connection
				$activeConnections.Remove($id)
			}
		}
	}
}

# Write all connections to output pipeline
$activeConnections.Values
$closedConnections
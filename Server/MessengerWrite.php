<?php
	if (isset($_REQUEST["isclient"]))
	{
		if ($_REQUEST["isclient"] == "true")
		{
			//Code here
		}
	}
	else
	{
?>
<!DOCTYPE html>
<html>
<head>
	<title>Warning!</title>
</head>
<body>
	<h1 style="font-family: Segoe UI; text-align: center; font-weight: lighter;">Client Read/Write only!</h1>
</body>
</html>
<?php
	}
?>
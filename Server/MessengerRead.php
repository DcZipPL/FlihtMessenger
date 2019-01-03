<?php
	if (isset($_REQUEST["isclient"]))
	{
		if ($_REQUEST["isclient"] == "true")
		{
			$filename = "./Messages.dat";
			$handle = fopen($filename, 'r');
			fseek($handle, $_REQUEST["byte"]);
			$readed = fread($handle, filesize($filename));
			echo(filesize($filename)."~".$readed);
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
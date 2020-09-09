<?php
//Upload
if(isset($_GET['u']))
{
	header('Content-Type: application/json;charset=utf-8');
	$data = json_decode(file_get_contents('php://input'), true);

	$fp = fopen('bots.json', 'w') or die("NoAccess");
	fwrite($fp, json_encode($data));
	fclose($fp);
} else if(isset($_GET['d'])) // Download
{
	header('Content-Type: application/json;charset=utf-8');
    	$data = NULL;
	
	if(file_exists('bots.json'))
	{
		if(file_get_contents('bots.json')) die(file_get_contents('bots.json'));
		else die("NoContent");
	}
    	else die("NoFile");
}
?>

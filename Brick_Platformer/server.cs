$Platformer::Root = "config/scripts/mod/Brick_Platformer/";
if(!isFile($Platformer::Root @ "server.cs"))
	$Platformer::Root = "Add-Ons/Brick_Platformer/";

$Platformer::Assets = $Platformer::Root @ "assets/";

function Platformer_LoadServer()
{
	exec("./server/main.cs");
}
Platformer_LoadServer();
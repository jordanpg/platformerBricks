$Platformer::Bumpers::ArrivalTolerance		= 7.5;
$Platformer::Bumpers::TargetUpdateRate		= 100;
$Platformer::Bumpers::TargetTraversalReq	= 1.5;
$Platformer::Bumpers::TargetStartOffset		= 0.75;
$Platformer::Bumpers::TargetMaxDistance		= 500;

function fxDTSBrickData::isValidBumper(%this)
{
	if(%this.platformerType !$= "Bumper")
		return false;

	if(%this.bumperPower <= 0)
		return false;

	return true;
}

function Player::useBumper(%this, %obj, %power, %dir, %setVel)
{
	%db = %obj.getDatablock();
	if(!%db.isValidBumper())
		return;

	if(%this.grinding)
		%this.grindExit();

	if(%power <= 0)
		%power = %db.bumperPower;

	if(VectorLen(%dir) == 0)
		%dir = "0 0 1";
	else
		%dir = VectorNormalize(%dir);

	%vel = VectorScale(%dir, %power);

	if(%setVel)
		%this.setVelocity(%vel);
	else
		%this.addVelocity(%vel);

	pDebug("BUMPER : Bumped player to velocity" SPC %this.getVelocity() SPC "(" @ %this @ ", " @ %obj @ ", " @ %power @ ", " @ %dir @ ", " @ %setVel @ ")", %this, %obj);

	ServerPlay3D(BumperSound, %obj.getPosition());

	%obj.onBumper(%this);
}

function Player::useDirectionalBumper(%this, %obj, %power, %dirID, %z, %setVel)
{
	%db = %obj.getDatablock();
	if(!%db.isValidBumper())
		return;

	if(%this.grinding)
		%this.grindExit();

	if(%power <= 0)
		%power = %db.bumperPower;

	if(%dirID $= "")
		%dirID = 0;

	switch(%dirID)
	{
		case 1: %dir = 3;
		case 2: %dir = 1;
		case 3: %dir = 0;
		case 4: %dir = 2;
		default:
			%dir = %obj.getAngleID() - 1;
			if(%dir < 0)
				%dir += 4;
	}

	%vel = rotateVector(%power SPC "0 0", "0 0 0", %dir);

	if(%z)
		%vel = VectorAdd(%vel, "0 0" SPC %power);

	if(%setVel)
		%this.setVelocity(%vel);
	else
	{
		%this.addVelocity(%vel);

		//This bit of code is to make sure that Z velocity is consistent.
		//Sometimes, velocity isn't immediately cancelled as expected upon hitting a brick, so we do this goofy thing to fix it.
		%zvP = getWord(%this.getVelocity(), 2);
		%zvV = getWord(%vel, 2);
		%this.addVelocity("0 0" SPC %zvV - %zvP);
	}

	pDebug("BUMPERDIR : Bumped player to velocity" SPC %this.getVelocity() SPC "(" @ %this @ ", " @ %obj @ ", " @ %power @ ", " @ %dir @ ", " @ %z @ ", " @ %setVel @ ")", %this, %obj);

	ServerPlay3D(BumperSound, %obj.getPosition());

	%obj.onBumper(%this);
}

function fxDTSBrick::useBumper(%this, %power, %dir, %setVel, %client)
{
	if(!isObject(%client.player))
		return;

	%client.player.useBumper(%this, %power, %dir, %setVel);
}

function fxDTSBrick::useDirectionalBumper(%this, %dirID, %power, %addZ, %setVel, %client)
{
	if(!isObject(%client.player))
		return;
		
	%client.player.useDirectionalBumper(%this, %power, %dirID, %addZ, %setVel);
}

function fxDTSBrick::onBumperArrived(%this, %sender, %player)
{
	$InputTarget_["Self"] = %this;
	$InputTarget_["Player"] = %player;
	$InputTarget_["Client"] = %player.client;
	$InputTarget_["Sender"] = %sender;

	//echo("arrived" SPC %this);
	%clientMini = getMinigameFromObject(%player.client);
	%selfMini = getMinigameFromObject(%this);
	if($Server::Lan)
		$InputTarget_["MiniGame"] = %clientMini;
	else
	{
		if(%clientMini == %selfMini)
			$InputTarget["MiniGame"] = %selfMini;
		else
			$InputTarget["MiniGame"] = 0;
	}
	%this.processInputEvent("onBumperArrived", %player.client);
}

function fxDTSBrick::onBumper(%this, %player)
{
	$InputTarget_["Self"] = %this;
	$InputTarget_["Player"] = %player;
	$InputTarget_["Client"] = %player.client;

	%clientMini = getMinigameFromObject(%player.client);
	%selfMini = getMinigameFromObject(%this);
	if($Server::Lan)
		$InputTarget_["MiniGame"] = %clientMini;
	else
	{
		if(%clientMini == %selfMini)
			$InputTarget["MiniGame"] = %selfMini;
		else
			$InputTarget["MiniGame"] = 0;
	}
	%this.processInputEvent("onBumper", %player.client);
}

package Platformer_Bumpers
{
	function fxDTSBrickData::onPlant(%this, %obj)
	{
		parent::onPlant(%this, %obj);

		if(%this.platformerType !$= "Bumper")
			return;

		if(!%this.bumperDirectional)
			%obj.addEvent(true, 0, "onPlayerTouch", "Self", "useBumper", 0, "0 0 0", 0);
		else
			%obj.addEvent(true, 0, "onPlayerTouch", "Self", "useDirectionalBumper", 0, 0, false, false);
	}
};
activatePackage(Platformer_Bumpers);

registerInputEvent(fxDTSBrick, "onBumperArrived", "Self fxDTSBrick\tPlayer Player\tClient GameConnection\tSender fxDTSBrick\tMiniGame MiniGame");
registerInputEvent(fxDTSBrick, "onBumper", "Self fxDTSBrick\tPlayer Player\tClient GameConnection\tMiniGame MiniGame");

registerOutputEvent(fxDTSBrick, "useBumper", "int 0 200 0\tvector\tlist AddVel 0 SetVel 1", true);
registerOutputEvent(fxDTSBrick, "useDirectionalBumper", "list Brick 0 North 1 South 2 East 3 West 4\tint 0 200 0\tbool\tlist AddVel 0 SetVel 1", true);
$Platformer::Bumpers::TargetUpdateRate		= 100;
$Platformer::Bumpers::TargetAntiGrav		= "0 0 0.4";
$Platformer::Bumpers::TargetStartOffset		= 0.75;
$Platformer::Bumpers::TargetMaxDistance		= 256;
$Platformer::Bumpers::TargetMaxOverTime		= 1;
$Platformer::Bumpers::TargetSearchFactor	= 0.5;
$Platformer::Bumpers::TargetMinimumSearch	= 4;

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

function Player::useTargetBumper(%this, %obj, %target, %time, %orientMode, %orient)
{
	if(!isObject(%obj) || !isObject(%target))
		return;

	if(%this.grinding)
		%this.grindExit();

	if(%this.bumping)
		%this.targetBumperExit();

	%start = %obj.getPosition();
	%end = %target.getPosition();

	%dist = VectorDist(%start, %end);
	if(%dist > $Platformer::Bumpers::TargetMaxDistance)
		return;

	%dir = VectorNormalize(VectorSub(%end, %start));
	%offset = VectorScale(%dir, $Platformer::Bumpers::TargetStartOffset);

	%this.setTransform(VectorAdd(%start, %offset) SPC getWords(%this.getTransform(), 3, 6));
	%this.setVelocity("0 0 0");

	if(%time <= 0)
		%time = 1;

	%dist = VectorDist(%start, %end);
	%speed = %dist / %time;

	switch(%orientMode)
	{
		case 1: //Absolute
			%this.bumperOrient = %orient;
		case 2: //Relative to Size
			%scale = %this.getScale();

			for(%i = 0; %i < 3; %i++)
				%o = %o SPC getWord(%orient, %i) * getWord(%scale, %i);

			%this.bumperOrient = ltrim(%o);
		case 3: //Top
			%this.bumperOrient = "0 0" SPC %target.getDatablock().brickSizeZ * 0.1;
		case 4: //Bottom
			%this.bumperOrient = "0 0" SPC -%target.getDatablock().brickSizeZ * 0.1;
		default:
			%this.bumperOrient = "";
	}

	// talk(%this.bumperOrient);

	%this.bumperStart = %start;
	%this.bumperEnd = %end;
	%this.bumperSender = nameToID(%obj);
	%this.bumperTarget = nameToID(%target);
	%this.bumperStartTime = $Sim::Time;
	%this.bumperTime = %time;
	%this.bumperSpeed = %speed;
	%this.bumperDistLast = $Platformer::Bumpers::TargetMaxDistance;
	%this.bumping = true;

	ServerPlay3D(BumperFarSound, %obj.getPosition());

	%this.bumpSchedule = %this.schedule(0, targetBumperStep);
}

function Player::targetBumperExit(%this)
{
	if(isEventPending(%this.bumpSchedule))
		cancel(%this.bumpSchedule);

	%this.bumping = false;
	%this.bumperStart = "";
	%this.bumperEnd = "";
	%this.bumperSender = "";
	%this.bumperTarget = "";
	%this.bumperStartTime = "";
	%this.bumperSpeed = "";
	%this.bumperSchedule = "";
	%this.bumperDistLast = "";
	%this.bumperOrient = "";
}

function Player::targetBumperStep(%this)
{
	if(isEventPending(%this.bumpSchedule))
		cancel(%this.bumpSchedule);

	if(!%this.bumping)
		return;

	if(!isObject(%this.bumperSender) || !isObject(%this.bumperTarget))
	{
		%this.targetBumperExit();
		return;
	}

	%pos = %this.getHackPosition();
	%dist = VectorDist(%pos, %this.bumperEnd);

	%this.bumperDir = VectorNormalize(VectorSub(%this.bumperEnd, %pos));
	%this.bumperVelocity = VectorScale(%this.bumperDir, %this.bumperSpeed);

	%sc = $Platformer::Bumpers::TargetSearchFactor * %this.bumperSpeed;
	// talk(%sc);
	if(%sc < $Platformer::Bumpers::TargetMinimumSearch)
		%sc = $Platformer::Bumpers::TargetMinimumSearch;
	%sc *= VectorLen(%this.getScale()) * 0.5;
	// talk(%sc);
	%vec = VectorScale(%this.bumperDir, %sc);
	%end = VectorAdd(%pos, %vec);
	%cast = containerRayCast(%pos, %end, $TypeMasks::FxBrickAlwaysObjectType);
	%obj = firstWord(%cast);

	if(%obj == %this.bumperTarget)
	{
		%this.targetBumperArrival();
		return;
	}

	if(%dist <= $Platformer::Bumpers::ArrivalTolerance)
	{
		%this.targetBumperArrival();
		return;
	}

	if($Sim::Time - (%this.bumperStartTime + %this.bumperTime) > $Platformer::Bumpers::TargetMaxOverTime) //Detects stuck players.
	{
		%this.targetBumperExit();
		return;
	}

	if(%this.bumperDistLast < %dist)
	{
		%this.targetBumperExit();
		return;
	}

	%this.setVelocity(VectorAdd(%this.bumperVelocity, $Platformer::Bumpers::TargetAntiGrav));

	%this.bumperDistLast = %dist;

	%this.bumpSchedule = %this.schedule($Platformer::Bumpers::TargetUpdateRate, targetBumperStep);
}

function Player::targetBumperArrival(%this)
{
	if(%this.bumperOrient !$= "")
		%this.setTransform(VectorAdd(%this.bumperEnd, %this.bumperOrient) SPC getWords(%this.getTransform(), 3, 6));

	%this.bumperTarget.onBumperArrived(%this.bumperSender, %this);

	%this.targetBumperExit();
}

function fxDTSBrick::useTargetBumper(%this, %target, %time, %orientMode, %orient, %client)
{
	if(!isObject(%client.player))
		return;

	%bg = getBrickGroupFromObject(%this);
	%nt = "_" @ %target;
	if(%bg.hasNTObject(%nt) $= -1)
		return;

	%target = %bg.NTObject[%nt, 0];
	if(!isObject(%target))
		return;

	%client.player.useTargetBumper(%this, %target, %time, %orientMode, %orient);
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
	function Armor::onTrigger(%this, %obj, %slot, %val)
	{
		parent::onTrigger(%this, %obj, %slot, %val);

		if(!%obj.bumping)
			return;

		if(%slot == 4 && %val)
			%obj.targetBumperExit();
	}

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
registerOutputEvent(fxDTSBrick, "useTargetBumper", "string 64 64\tfloat 0.25 10 0.25 1\tlist NoOrient 0 Absolute 1 Relative_Size 2 Up 3\tvector", true);
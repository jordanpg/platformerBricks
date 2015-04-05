//GLOBALS
$Platformer::Bumper::ArrivedDist = 7.5; //Distance when BumperTo considers the player to have reached the target.
$Platformer::Bumper::Speed = 100; //Speed of the BumperTo tick. Be very careful changing this, low times can cause players to stop mid-way, high times can cause players to miss the target.
$Platformer::Bumper::WorkingDist = 1.5; //Distance the player has to move in a targeted bumper path to be considered moving on a working path.
$Platformer::Bumper::AddedZ = 0.75; //Distance above bumper that the player starts at when using a targeted bumper.
$Platformer::Bumper::MaxDist = 500; //Maximum distance from point A to point B with target bumpers.
$Platformer::Bumper::TargetAddZ = 2; //Used to orient players upon arrival at target brick.


//DEPENDENCY FUNCTIONS
function SimGroup::hasNTObject(%bg, %nt) //Fairly-self-explanatory, returns an index if the brick name is found in a brickgroup.
{
	for(%i = 0; %i < %bg.NTNameCount; %i++)
	{
		if(%bg.NTName[%i] $= %nt)
			return %i;
	}
	return -1;
}

//CLASS FUNCTIONS
function fxDTSBrick::BumperTo(%this, %target, %player, %start, %orient) //Target bumper function.
{
	if(isEventPending(%player.bumper))
		cancel(%player.bumper);

	if(!isObject(%player))
		return;

	if(%player.grinding) //Detach player from rails.
		%player.grindExit();

	//echo(%target SPC %target.getName());

	if(%start) //Mostly used to correct positions so that paths are more consistent and functional.
	{
		ServerPlay3D(BumperFarSound, %this.getPosition());
		if(%this.getDatablock().bumperDirectional)
		{
			%vec = $Platformer::Bumper::AddedZ SPC "0 0";
			%ang = %this.getAngleID() - 1;
			if(%ang < 0)
				%ang += 4;
			%player.position = VectorAdd(%this.getPosition(), rotateVector(%vec, "0 0 0", %ang));
		}
		else
			%player.position = VectorAdd(%this.getPosition(), "0 0" SPC $Platformer::Bumper::AddedZ);
	}

	%srcPos = %player.getPosition();
	%tarPos = %target.getPosition();
	%dist = VectorDist(%tarPos, %srcPos);
	if(%dist > $Platformer::Bumper::MaxDist) //Stop if the distance is too great.
	{
		%player.bumperLastDist = "";
		%player.bumperMoving = false;
		return;
	}

	//echo("ww" SPC %target SPC %target.getName() SPC %this SPC %this.getName());
	if(%dist < $Platformer::Bumper::ArrivedDist) //Detects if the player has arrived or if the path is too long.
	{
		//echo("aa" SPC %dist SPC %target);
		%target.onBumperArrived(%this, %player);
		%player.setVelocity("0 0 0");
		%player.bumperLastDist = "";
		%player.bumperMoving = false;
		if(%orient)
		{
			if(%target.getDatablock().bumperDirectional) //Directional bumpers need to orient in front of themselves, otherwise players get stuck inside.
			{
				%vec = $Platformer::Bumper::TargetAddZ SPC "0 0";
				%ang = %target.getAngleID() - 1;
				if(%ang < 0)
					%ang += 4;
				%player.position = VectorAdd(%tarPos, rotateVector(%vec, "0 0 0", %ang));
			}
			else
				%player.position = VectorAdd(%tarPos, "0 0" SPC $Platformer::Bumper::TargetAddZ);
		}
		return;
	}

	if(%sub = (%player.bumperLastDist - %dist) < $Platformer::Bumper::WorkingDist && %player.bumperMoving) //This bit will detect if the player's stuck, along with certain unfortunate side-effects with lag.
	{
		//echo("bogey" SPC %player.bumperLastDist);
		%player.bumperLastDist = "";
		%player.bumperMoving = false;
		return;
	}

	//this math makes the player move toward the target
	%scale = (mFloor((%dist / 5) + 0.5) + 1) * 10;
	%diff = VectorSub(%tarPos, %srcPos);
	%norm = VectorNormalize(%diff);
	%vel = VectorScale(%norm, %scale);
	%player.setVelocity(%vel);

	%player.bumperMoving = true;
	%player.bumperLastDist = %dist;
	%player.bumper = %this.schedule($Platformer::Bumper::Speed - %this.getDatablock().bumperAddSpeed, BumperTo, %target, %player, false, %orient);
}

function fxDTSBrick::Bumper(%this, %player, %power, %val) //Handles regular up/down bumpers.
{
	if(isEventPending(%player.bumper) && !%player.bumperMoving)
		cancel(%player.bumper);

	if(%player.grinding)
		%player.grindExit();

	if(%val)
	{
		if(%power > 0)
			%vel = %power;
		else
			%vel = %this.getDatablock().bumperPower;

		%player.setVelocity("0 0" SPC %vel);
		ServerPlay3D(BumperSound, %this.getPosition());
		return;
	}
	else
		%this.onBumper(%player);

	%player.bumper = %this.schedule($Platformer::Bumper::Speed, Bumper, %player, %power, true);
}

function fxDTSBrick::BumperDir(%this, %player, %power, %direction, %z, %val) //Handles directional bumpers.
{
	if(isEventPending(%player.bumper) && !%player.bumperMoving)
		cancel(%player.bumper);

	if(%player.grinding)
		%player.grindExit();

	if(%val)
	{
		if(%power > 0)
			%vel = %power;
		else
			%vel = %this.getDatablock().bumperPower;

		if(%direction != -1)
			%velocity = rotateVector(%vel SPC "0 0", "0 0 0", %direction);
		else
			%velocity = VectorScale(VectorNormalize(getWords(%player.getVelocity(), 0, 1) SPC "0"), %vel);

		if(%z)
			%velocity = vectorAdd(%velocity, "0 0" SPC %vel);

		%player.setVelocity(%velocity);
		ServerPlay3D(BumperSound, %this.getPosition());
		return;
	}
	else
		%this.onBumper(%player);

	%player.bumper = %this.schedule(0, BumperDir, %player, %power, %direction, %z, true);
}

function fxDTSBrick::doBumper(%this, %power, %client)
{
	if(!isObject(%client.player))
		return;

	%this.Bumper(%client.player, %power, true);
}

function fxDTSBrick::targetBumper(%this, %name, %orient, %client)
{
	if(!isObject(%client.player))
		return;

	%bg = getBrickGroupFromObject(%this);
	%nt = "_" @ %name;
	if(%bg.hasNTObject(%nt) $= -1)
		return;

	%target = %bg.NTObject[%nt, 0];
	if(!isObject(%target))
		return;

	//echo(%target SPC %nt);
	%this.BumperTo(%target, %client.player, true, %orient);
}

function fxDTSBrick::doBumperDir(%this, %dir, %power, %addZ, %client)
{
	if(!isObject(%client.player))
		return;

	switch(%dir)
	{
		case 0: %dir = -1;
		case 1: %dir = 3;
		case 2: %dir = 1;
		case 3: %dir = 0;
		case 4: %dir = 2;
		case 5:
			%dir = %this.getAngleID() - 1;
			if(%dir < 0)
				%dir += 4;
	}

	%this.BumperDir(%client.player, %power, %dir, %addZ, true);
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


//Package
package Platformer_Bumpers
{
	function fxDTSBrickData::onPlayerTouch(%this, %obj, %player)
	{
		parent::onPlayerTouch(%this, %obj, %player);

		if(%this.platformerType !$= "Bumper")
			return;

		if(!%this.bumperDirectional)
			%obj.Bumper(%player);
		else
		{
			%ang = %obj.getAngleID() - 1; //we're subtracting one because the sideways bumper bricks don't face where their angleID rotates the vector to.
			if(%ang < 0)
				%ang += 4;
			%obj.BumperDir(%player, 0, %ang, true);
		}
	}
};
activatePackage(Platformer_Bumpers);


//EVENT REGISTRATION
registerInputEvent(fxDTSBrick, "onBumperArrived", "Self fxDTSBrick\tPlayer Player\tClient GameConnection\tSender fxDTSBrick\tMiniGame MiniGame");
registerInputEvent(fxDTSBrick, "onBumper", "Self fxDTSBrick\tPlayer Player\tClient GameConnection\tMiniGame MiniGame");

registerOutputEvent(fxDTSBrick, "doBumper", "int 0 200 0", true);
registerOutputEvent(fxDTSBrick, "targetBumper", "string 64 64\tbool", true);
registerOutputEvent(fxDTSBrick, "doBumperDir", "list Relative 0 North 1 South 2 East 3 West 4 Brick 5\tint 0 200 0\tbool", true);
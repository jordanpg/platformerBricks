//GLOBALS
$Platformer::Booster::BoosterEventWait = 100; //A schedule is used to slightly delay the booster's default function. This allows for the doBooster event to work alongside normal boosters.
$Platformer::Booster::AddZVel = 0.5; //Upwards velocity added with boosters to help out with friction.
$Platformer::Booster::Timeout = 500; //So players don't get boosted tons of times.
$Platformer::Booster::UpDivFactor = 25; //The velocity of a booster is divided by this to scale the upwards velocity, allowing for better boosting results.


//DATABLOCKS
datablock AudioProfile(BoosterSound)
{
	description = "AudioClosest3D";
	fileName = "./resources/booster.wav";
	preload = true;
};

datablock fxDTSBrickData(BrickBoosterData)
{
	brickFile = "./resources/booster.blb";
	iconName = "config/scripts/server/Brick_Platformer/resources/icon_booster";

	category = "Special";
	subCategory = "Platformer - Misc";
	uiName = "Player Booster";

	isBooster = true; //Flag for boosters.
	boosterPower = 50; //Velocity that the player is sped up by.
};


//CLASS FUNCTIONS
function fxDTSBrick::Booster(%this, %player, %power, %do) //Makes boosters boost. Gotta go fast.
{
	if(isEventPending(%player.booster))
		cancel(%player.booster);

	if(!isObject(%player))
		return;

	if(getSimTime() - %player.lastBoost < $Platformer::Booster::Timeout)
		return;

	%db = %this.getDatablock();
	if(!%db.isBooster)
		return;
	if(%do)
	{
		if(%power > 0)
			%vel = %power;
		else
			%vel = %db.boosterPower;

		%addvel = "0 0" SPC $Platformer::Booster::AddZVel;
		%addvel = VectorScale(%addvel, (%vel / $Platformer::Booster::UpDivFactor) + 1);

		%ang = %this.getAngleID() + 1;
		if(%ang > 3)
			%ang -= 4;

		%vel = %vel SPC "0 0";
		%vel = rotateVector(%vel, "0 0 0", %ang);
		%vel = VectorAdd(%vel, %addvel);
		%player.addVelocity(%vel);
		ServerPlay3D(BoosterSound, %this.getPosition());
		%player.lastBoost = getSimTime();
	}
	else
	{
		%this.onBooster(%player);
		%player.booster = %this.schedule($Platformer::Booster::BoosterEventWait, Booster, %player, %power, true);
	}
}

function fxDTSBrick::doBooster(%this, %power, %client) //The event that allows for custom power. Gotta go faster.
{
	if(!isObject(%client.player))
		return;

	if(!%this.getDatablock().isBooster)
		return;

	%this.Booster(%client.player, %power, true);
}

function fxDTSBrick::onBooster(%this, %player)
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
	%this.processInputEvent("onBooster", %player.client);
}

//PACKAGE
package Platformer_Booster
{
	function fxDTSBrickData::onPlant(%this, %obj)
	{
		parent::onPlant(%this, %obj);

		if(%this.isBooster)
			%obj.enableTouch = true;
	}

	function fxDTSBrickData::onLoadPlant(%this, %obj)
	{
		parent::onLoadPlant(%this, %obj);

		if(%this.isBooster)
			%obj.enableTouch = true;
	}

	function fxDTSBrickData::onPlayerTouch(%this, %obj, %player)
	{
		parent::onPlayerTouch(%this, %obj, %player);

		if(!%this.isBooster)
			return;

		%obj.Booster(%player, 0, false);
	}
};
activatePackage(Platformer_Booster);


//EVENT REGISTRATION
registerInputEvent(fxDTSBrick, "onBooster", "Self fxDTSBrick\tPlayer Player\tClient GameConnection\tMiniGame MiniGame");
registerOutputEvent(fxDTSBrick, "doBooster", "int 0 200 0", true);
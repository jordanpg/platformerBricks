function fxDTSBrickData::isBooster(%this)
{
	if(!isObject(%this))
		return false;

	if(%this.platformerType !$= "Booster")
		return false;

	if(%this.boosterPower <= 0)
		return false;

	return true;
}

function Player::boosterInit(%this, %obj, %amount, %period)
{
	if(isEventPending(%this.boostCancel))
		cancel(%this.boostCancel);

	if(%this.boosting && !$boosters::FAST)
		%this.boosterStop();

	%this.boosting = true;
	%this.boostObj = %obj;
	%this.boostAmt = %amount;
	%this.boostPeriod = %period;

	%this.modMoveSpeeds(%amount);

	%obj.onBooster(%this);

	ServerPlay3D(BoosterSound, %this.getPosition());

	%this.boostCancel = %this.schedule(%period * 1000, boosterStop, true);
}

function Player::boosterStop(%this, %timeout)
{
	if(isEventPending(%this.boostCancel))
		cancel(%this.boostCancel);

	%this.modMoveSpeeds(-%this.boostAmt);

	if(%timeout)
		%this.boostObj.onBoosterFinish(%this);
	else
		%this.boostObj.onBoosterCancel(%this);

	%this.boosting = false;
	%this.boostObj = "";
	%this.boostAmt = "";
	%this.boostPeriod = "";
	%this.boostCancel = "";
}

function fxDTSBrick::useBooster(%this, %amount, %period, %client)
{
	if(!isObject(%client.player))
		return;

	%client.player.boosterInit(%this, %amount, %period);
}

function fxDTSBrick::cancelBooster(%this, %client)
{
	if(!isObject(%client.player))
		return;

	if(!%client.player.boosting)
		return;

	%client.player.boosterStop();
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

function fxDTSBrick::onBoosterFinish(%this, %player)
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
	%this.processInputEvent("onBoosterFinish", %player.client);
}

function fxDTSBrick::onBoosterCancel(%this, %player)
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
	%this.processInputEvent("onBoosterCancel", %player.client);
}

package Platformer_Boosters
{
	function fxDTSBrickData::onPlant(%this, %obj)
	{
		parent::onPlant(%this, %obj);

		if(%this.platformerType !$= "Booster")
			return;

		%obj.addEvent(true, 0, "onPlayerTouch", "Self", "useBooster", %this.boosterPower, 3);
	}
};
activatePackage(Platformer_Boosters);

registerInputEvent(fxDTSBrick, "onBooster", "Self fxDTSBrick\tPlayer Player\tClient GameConnection\tMiniGame MiniGame");
registerInputEvent(fxDTSBrick, "onBoosterFinish", "Self fxDTSBrick\tPlayer Player\tClient GameConnection\tMiniGame MiniGame");
registerInputEvent(fxDTSBrick, "onBoosterCancel", "Self fxDTSBrick\tPlayer Player\tClient GameConnection\tMiniGame MiniGame");

registerOutputEvent(fxDTSBrick, "useBooster", "float 0.5 100 0.5 50\tfloat 0 30 0.25 3", true);
registerOutputEvent(fxDTSBrick, "cancelBooster", "", true);
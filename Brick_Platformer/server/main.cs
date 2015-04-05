exec("./datablocks.cs");
exec("./dependency.cs");
exec("./grindRails.cs");
//TODO: Fix up bumpers and boosters; move datablocks to datablocks.cs with correct paths
exec("./bumper2.cs");
// exec("./booster.cs");

package PlatformerBricks_Server
{
	function fxDTSBrickData::onPlant(%this, %obj)
	{
		parent::onPlant(%this, %obj);

		if(%this.platformerType !$= "")
			%obj.enableTouch = true;
	}

	function fxDTSBrickData::onLoadPlant(%this, %obj)
	{
		parent::onLoadPlant(%this, %obj);

		if(%this.platformerType !$= "")
			%obj.enableTouch = true;
	}
};
activatePackage(PlatformerBricks_Server);
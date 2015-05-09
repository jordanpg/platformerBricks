exec("./datablocks.cs");
exec("./dependency.cs");
exec("./grindRails.cs");
exec("./bumper2.cs");
exec("./booster2.cs");

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
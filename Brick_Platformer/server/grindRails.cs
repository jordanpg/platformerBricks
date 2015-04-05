//Grind Rails System v2
//Author: ottosparks
//To replace the prior inefficient single-segment rail system with a more straightforward and intuitive variable-length system.

//Global preferences that alter how rails function. These shouldn't be messed with probably. se prohibe cambiarlos!!
$Platformer::Rails::DefaultSpeed 		= 10; 			//Default speed at which a player will move on a rail. Modifiable per-datablock, per-section.
$Platformer::Rails::DefaultUpdateRate 	= 50; 			//Default speed at which a rail will run its tick while a player grinds on it. Modifiable per-datablock.
$Platformer::Rails::PlayerFixVector 	= "0 0 0.25";	//Used to "fix" player position relative to rail points. This is added to the point so the player isn't stuck in the brick.
$Platformer::Rails::PlayerGravityFix	= 20;			//Used to counteract gravity whilst on rails.
$Platformer::Rails::TouchTimeout 		= 0.05;			//Timeout between rail touches
$Platformer::Rails::MaximumDeviance		= 0.5;			//Maximum amount that a player can be away from the expected position.
$Platformer::Rails::GhostShapeUpdate	= 50;			//Period between each ghost arrow update.
$Platformer::Rails::GhostShapeInc		= 0.01;			//Amount arrow moves each update
$Platformer::Rails::DefaultExitSpeed	= 12.5;			//Speed when jumping from a rail
$Platformer::Rails::DefaultJumpSpeed	= 16;			//Upward speed applied when jumping from a rail
$Platformer::Rails::MaxSwitchDist		= 2.5;			//Maximum rail switch distance

function fxDTSBrickData::isValidRail(%this)
{
	if(%this.platformerType !$= "Rail") //Rail datablocks must have platformerType set to "Rail"
		return false;

	if(%this.railPoint0 $= "" || %this.railPoint1 $= "") //Rail datablocks must have at least two rail points.
		return false;

	if(%this.railEndSearchDir $= "" || %this.railStartSearchDir $= "") //Rail datablocks must provide a direction for a ray search from their end and start points, which is used to find the next rail.
		return false;

	return true;
}

function fxDTSbrickData::gGetSegmentCount(%this)
{
	%i = 0;
	while(%this.railPoint[%i] !$= "")
		%i++;

	return %i;
}

function fxDTSBrickData::gGetSegmentStart(%this, %i, %r)
{
	if(%r)
	{
		if(%this.railPoint[%i] $= "")
			return -1; //non-existent point
		if(%this.railPoint[%i-1] $= "")
			return -2; //terminal point, doesn't start a section

		return %this.railPoint[%i];
	}
	if(%this.railPoint[%i] $= "")
		return -1; //non-existent point
	if(%this.railPoint[%i+1] $= "")
		return -2; //terminal point, doesn't start a section

	return %this.railPoint[%i];
}

function fxDTSBrickData::gGetSegmentEnd(%this, %i, %r)
{
	if(%r)
	{
		if(%this.railPoint[%i] $= "")
			return -1; //non-existent start point
		if(%this.railPoint[%i-1] $= "")
			return -2; //start point is terminal, no end

		return %this.railPoint[%i-1];
	}
	if(%this.railPoint[%i] $= "")
		return -1; //non-existent start point
	if(%this.railPoint[%i+1] $= "")
		return -2; //start point is terminal, no end

	return %this.railPoint[%i+1];
}

function fxDTSBrickData::gGetTotalLength(%this)
{
	%last = %this.railPoint0;
	if(%last $= "")
		return -1;

	%i = 0;
	%dist = 0;
	for(%point = %this.railPoint1; %point !$= ""; %point = %this.railPoint[%i++])
	{
		%dist += VectorDist(%point, %last);
		%last = %point;
	}
	return %dist;
}

function fxDTSBrickData::gGetSegmentLength(%this, %i, %r)
{
	if(%r)
	{
		%start = %this.railPoint[%i];
		if(%start $= "")
			return -1;

		%end = %this.railPoint[%i-1];
		if(%end $= "")
			return -2;

		return VectorDist(%start, %end);
	}
	%start = %this.railPoint[%i];
	if(%start $= "")
		return -1;

	%end = %this.railPoint[%i+1];
	if(%end $= "")
		return -2;

	return VectorDist(%start, %end);
}

function fxDTSBrickData::gGetSegmentDirection(%this, %i, %r)
{
	if(%r)
	{
		%start = %this.railPoint[%i];
		if(%start $= "")
			return -1;

		%end = %this.railPoint[%i-1];
		if(%end $= "")
			return -2;

		return VectorNormalize(VectorSub(%end, %start));
	}
	%start = %this.railPoint[%i];
	if(%start $= "")
		return -1;

	%end = %this.railPoint[%i+1];
	if(%end $= "")
		return -2;

	return VectorNormalize(VectorSub(%end, %start));
}

function fxDTSBrickData::gGetSegmentSpeed(%this, %i, %r)
{
	if(%r)
	{
		if(%this.railPoint[%i] $= "" || %this.railPoint[%i-1] $= "") //no speed if we aren't going anywhere!
			return -1;

		return (%this.railSpeed[%i-1] !$= "" ? %this.railSpeed[%i-1] : $Platformer::Rails::DefaultSpeed);
	}

	if(%this.railPoint[%i] $= "" || %this.railPoint[%i+1] $= "") //no speed if we aren't going anywhere!
		return -1;

	return (%this.railSpeed[%i] !$= "" ? %this.railSpeed[%i] : $Platformer::Rails::DefaultSpeed);
}

function fxDTSBrickData::gGetSegmentVelocity(%this, %i, %r)
{
	%speed = %this.gGetSegmentSpeed(%i, %r);
	if(%speed < 0)
		return %speed;

	%dir = %this.gGetSegmentDirection(%i, %r);
	if(%dir < 0)
		return %dir-1;

	return VectorScale(%dir, %speed);
}

function fxDTSBrickData::gGetAverageSpeed(%this)
{
	%i = 1;
	%speed = 0;
	for(%seg = %this.gGetSegmentSpeed(%i); %seg != -1; %seg = %this.gGetSegmentSpeed(%i++))
		%speed += %seg;

	return %speed / %i;
}

function fxDTSBrickData::gGetUpdateRate(%this)
{
	return (%this.railUpdateRateMS !$= "" ? %this.railUpdateRateMS : $Platformer::Rails::DefaultUpdateRate);
}

function fxDTSBrick::gGetRailPoint(%this, %i, %fixPlayer, %reverse)
{
	if((%fi = (%reverse ? mCeil(%i) : mFloor(%i))) == %i)
	{
		%rel = %this.getDatablock().railPoint[%fi];
		if(%rel $= "")
			return -1;

		%pos = %this.getPosition();
		%rel = rotateVector(%rel, "0 0 0", %this.getAngleID());
		%point = VectorAdd(%pos, %rel);
	}
	else
	{
		%rel1 = %this.getDatablock().railPoint[%fi];
		if(%rel1 $= "")
			return -2;
		%rel2 = %this.getDatablock().railPoint[(%reverse ? %fi-1 : %fi+1)];
		if(%rel2 $= "")
			return -3;

		%pos = %this.getPosition();
		%rel1 = rotateVector(%rel1, "0 0 0", %a = %this.getAngleID());
		%rel2 = rotateVector(%rel2, "0 0 0", %a);

		if(%reverse)
			%point = VectorAdd(%pos, VectorInterpolate_Linear(%rel1, %rel2, %fi - %i));
		else
			%point = VectorAdd(%pos, VectorInterpolate_Linear(%rel1, %rel2, %i - %fi));
	}

	if(%fixPlayer)
	{
		// talk(%point);
		%point = VectorAdd(%point, $Platformer::Rails::PlayerFixVector);
		// talk(%point);
	}

	return %point;
}

function fxDTSBrick::gGetRailVelocity(%this, %i, %fixPlayer, %reverse)
{
	%db = %this.getDatablock();
	if((%fi = (%reverse ? mCeil(%i) : mFloor(%i))) == %i)
	{
		%vel = %db.gGetSegmentVelocity(%fi, %reverse);
		if(getWordCount(%vel) != 3)
			return -1;
	}
	else
	{
		// talk(%i SPC %fi SPC %reverse);
		%vel1 = %db.gGetSegmentVelocity(%fi, %reverse);
		if(getWordCount(%vel1) != 3)
			return -2;
		%vel2 = %db.gGetSegmentVelocity((%reverse ? %fi-1 : %fi+1), %reverse);
		if(getWordCount(%vel2) != 3)
		{
			if(%db.railPoint[(%reverse ? %fi-1 : %fi+1)] $= "")
				return -3;
			%vel = %vel1;
		}
		else
		{
			if(%reverse)
				%vel = VectorInterpolate_Linear(%vel1, %vel2, %fi - %i);
			else
				%vel = VectorInterpolate_Linear(%vel1, %vel2, %i - %fi);
		}
	}

	%vel = rotateVector(%vel, "0 0 0", %this.getAngleID());

	if(%fixPlayer)
	{
		%scale = (%rate !$= "" ? %rate : %db.gGetUpdateRate()) / 1000;
		// talk(%vel SPC %scale);
		%vel = VectorAdd(%vel, "0 0" SPC $Platformer::Rails::PlayerGravityFix * %scale);
		// talk(%vel);
	}

	return %vel;
}

function fxDTSBrick::gEndSearch(%this, %reverse)
{
	pDebug("GRINDSEARCH : Brick initiating search (" @ %this @ ")", %this);
	pDebug("   -Reverse:" SPC (%reverse ? "yes" : "no"), %this);
	%db = %this.getDatablock();
	%len = %db.gGetSegmentCount();
	if(!%reverse)
	{
		%start = %this.gGetRailPoint(%len-1);
		%dir = %db.railEndSearchDir;
		
		%search = RelayCheck(%start, %this.getAngleID(), 0.5, %dir);
	}
	else
	{
		%start = %this.gGetRailPoint(0);
		%dir = %db.railStartSearchDir;

		%search = RelayCheck(%start, %this.getAngleID(), 0.5, %dir);
	}
	pDebug("   -Start:" SPC %start, %this);
	pDebug("   -Dir:" SPC %dir, %this);
	pDebug("   -Search:" SPC %search, %this);
	return %search;
}

function Player::grindEnter(%this, %obj, %startPoint, %reverse)
{
	if(!isObject(%obj) || !isObject(%this))
		return false;

	%db = %obj.getDataBlock();
	if(!%db.isValidRail())
		return false;

	pDebug("GRIND : Player entering grind (" @ $Sim::Time @ ", " @ %this @ ", " @ %obj @ ")", %this, %obj);

	if(%startPoint $= "")
	{
		%len = %db.gGetTotalLength();
		%start = %obj.gGetRailPoint(0, 1);
		%dist = VectorDist(%start, %this.getPosition());

		pDebug("   -Len:" SPC %len, %obj, %this);
		pDebug("   -Start:" SPC %start, %obj, %this);
		pDebug("   -Pos:" SPC %this.getPosition(), %obj, %this);
		pDebug("   -Dist:" SPC %dist, %obj, %this);

		if(%dist > %len)
		{
			pDebug("   -Reverse; distance exceeds length", %obj, %this);
			%this.grindReversed = true;
		}
		%startPoint = %dist / %len;

		pDebug("   -startPoint:" SPC %startPoint, %obj, %this);
	}

	%objDir = rotateVector(%db.railEndSearchDir, "0 0 0", %obj.getAngleID());
	%comp = mFloatLength(VectorDot(%this.getVelocity(), %objDir), 0);
	if(%comp < 0)
		%this.grindReversed = true;

	if(%reverse !$= "")
		%this.grindReversed = %reverse;

	if(%startPoint < 0)
		%startPoint = 0;

	if(%startPoint > (%ct = %db.gGetSegmentCount()-1))
		%startPoint = %ct;

	%this.grinding = true;
	%this.grindObj = %obj;
	%this.grindPointLast = %startPoint;
	%this.grindCurrSegment = (%this.grindReversed ? mCeil(%startPoint) : mFloor(%startPoint));

	%pt = %obj.gGetRailPoint(%startPoint, 1, %this.grindReversed);
	%this.setTransform(%pt SPC getWords(%this.getTransform(), 3, 6));
	%this.setVelocity(%obj.gGetRailVelocity(%startPoint, 1, %this.grindReversed));

	
	pDebug("   -Reverse:" SPC (%this.grindReversed ? "yes" : "no"), %this, %obj);

	ServerPlay3D(RailContactSound, %pt);
	%this.playAudio(3, RailGrindSound);

	%this.grindSchedule = %this.scheduleNoQuota(0, grindStep, %obj);
}

function Player::grindTransfer(%this, %obj, %new, %startPoint, %reverse)
{
	if(isEventPending(%this.grindSchedule))
		cancel(%this.grindSchedule);

	if(!isObject(%obj) || !isObject(%new) || !%this.grinding)
		return false;

	%db = %obj.getDatablock();
	%dbnew = %new.getDatablock();
	if(!%db.isValidRail() || !%dbnew.isValidRail())
		return false;

	pDebug("GRIND : Transferring control (" @ $Sim::Time @ ", " @ %this @ ", " @ %obj @ "->" @ %new @ ")", %this, %obj);

	%dirOld = rotateVector(%db.railEndSearchDir, "0 0 0", %obj.getAngleID());
	%dirNew = rotateVector(%dbnew.railEndSearchDir, "0 0 0", %new.getAngleID());
	%comp = mFloatLength(VectorDot(%dirOld, %dirNew), 0);
	if(%comp < 0)
		%this.grindReversed ^= 1;

	pDebug("   -Comp:" SPC %comp, %this, %obj, %new);

	if(%startPoint $= "")
	{
		%startNew = %new.gGetRailPoint(0);
		%endNew = %new.gGetRailPoint(%endLast = %dbnew.gGetSegmentCount()-1);
		%pos = %this.getPosition();
		%distA = VectorDist(%startNew, %pos);
		%distB = VectorDist(%endNew, %pos);
		%lenNew = %dbNew.gGetTotalLength();

		pDebug("   -Len:" SPC %lenNew, %obj, %this, %new);
		pDebug("   -Start:" SPC %startNew, %obj, %this, %new);
		pDebug("   -Pos:" SPC %this.getPosition(), %obj, %this, %new);
		pDebug("   -DistA:" SPC %distA, %obj, %this, %new);
		pDebug("   -DistB:" SPC %distB, %obj, %this, %new);

		if(%reverse $= "")
		{
			if(%distA > %distB)
				%this.grindReversed = true;
			else
				%this.grindReversed = false;
		}
		else
			%this.grindReversed = %reverse;
		
		%start = %distA / %lenNew;
		if(!%this.grindReversed)
			%start = -%start;

		%this.grindPointLast = %start;
		%this.grindCurrSegment = (%this.grindReversed ? mCeil(%start) : mFloor(%start));
		if(%this.grindCurrSegment < 0)
			%this.grindCurrSegment = 0;
		if(%this.grindCurrSegment > %endLast)
			%this.grindCurrSegment = %endLast;
	}
	else
	{
		// %objDir = rotateVector(%dbnew.railEndSearchDir, "0 0 0", %new.getAngleID());
		// %comp = mFloatLength(VectorDot(%this.getVelocity(), %objDir), 0);
		// if(%comp < 0)
		// 	%this.grindReversed = true;

		// pDebug("   -objDir:" SPC %objDir, %this, %obj, %new);
		// pDebug("   -comp:" SPC %comp, %this, %obj, %new);

		if(%reverse !$= "")
			%this.grindReversed = %reverse;

		if(%startPoint < 0)
			%startPoint = 0;

		if(%startPoint > (%ct = %db.gGetSegmentCount()-1))
			%startPoint = %ct;

		%this.grindPointLast = %startPoint;
		%this.grindCurrSegment = (%this.grindReversed ? mCeil(%startPoint) : mFloor(%startPoint));
	}

	%this.grindObj = %new;

	pDebug("   -Reverse:" SPC (%this.grindReversed ? "yes" : "no"), %this, %obj);
	pDebug("   -PointLast:" SPC %this.grindPointLast, %obj, %this, %new);
	pDebug("   -CurrSegment:" SPC %this.grindCurrSegment, %obj, %this, %new);

	%rate = %dbNew.gGetUpdateRate();
	%this.grindSchedule = %this.scheduleNoQuota(%rate, grindStep, %new);
}

function Player::tryGrindTransfer(%this, %obj)
{
	%new = %obj.gEndSearch(%this.grindReversed);
	if(!isObject(%new))
		return -1;

	%this.grindTransfer(%obj, %new);

	return %new;
}

function Player::grindStep(%this, %obj)
{
	if(isEventPending(%this.grindSchedule))
		cancel(%this.grindSchedule);

	if(!%this.grinding)
		return;

	if(!isObject(%obj))
	{
		%this.grindExit();
		return;
	}

	%db = %obj.getDatablock();
	if(!%db.isValidRail())
		return;

	pDebug("GRIND : Grind step (" @ $Sim::Time @ ", " @ %this @ ", " @ %obj @ ")", %this, %obj);
	pDebug("   -Last Point:" SPC %this.grindPointLast, %this, %obj);
	pDebug("   -Curr Segment:" SPC %this.grindCurrSegment, %this, %obj);
	pDebug("   -Reverse:" SPC (%this.grindReversed ? "yes" : "no"), %this, %obj);

	%len = %db.gGetSegmentLength(%this.grindCurrSegment, %this.grindReversed);
	%dist = VectorDist(%pos = %this.getPosition(), %obj.gGetRailPoint(%this.grindCurrSegment, 1, %this.grindReversed));
	%mu = %dist / %len;
	%currPoint = %this.grindCurrSegment + (%this.grindReversed ? -%mu : %mu);

	%expectedPos = %obj.gGetRailPoint(%currPoint, 1, %this.grindReversed);
	if(VectorDist(%pos, %expectedPos) >= $Platformer::Rails::MaximumDeviance)
		%setPos = true;

	%rate = %db.gGetUpdateRate();
	%speed = %db.gGetSegmentSpeed(%this.grindCurrSegment, %this.grindReversed);
	%nextPoint = %obj.gGetRailPoint(%this.grindCurrSegment+(%this.grindReversed ? -1 : 1), 1);
	pDebug("   -NextPoint:" SPC %nextPoint, %obj, %this);
	%distLeft = VectorDist((%setPos ? %expectedPos : %pos), %nextPoint);
	%time = mFloor(%distLeft / %speed * 1000);
	if(%time < %rate)
		%rate = %time;

	%expectedVel = %obj.gGetRailVelocity(%currPoint, 1, %this.grindReversed, %rate);

	pDebug("   -currPoint:" SPC %currPoint, %this, %obj);
	pDebug("   -expectedPos:" SPC %expectedPos, %this, %obj);
	pDebug("   -expectedVel:" SPC %expectedVel, %this, %obj);

	if(getWordCount(%expectedPos) != 3 || getWordCount(%expectedVel) != 3)
	{
		//TODO: Implement transferring grind control to next brick
		pDebug("GRIND : Exiting grind; position/velocity check returned negative (" @ %this @ ", " @ %obj @ ")", %this, %obj);

		if(!isObject(%this.tryGrindTransfer(%obj)))
			%this.grindExit(%obj);
		return;
	}

	if(%setPos)
		%this.setTransform(%expectedPos SPC getWords(%this.getTransform(), 3, 6));
	%this.setVelocity(%expectedVel);

	pDebug("   -PlayerPos:" SPC %this.getPosition(), %this, %obj);
	pDebug("   -PlayerVel:" SPC %this.getVelocity(), %this, %obj);


	%this.grindPointLast = %currPoint;
	if(%mu > 1)
	{
		%p = %this.grindCurrSegment;
		%this.grindCurrSegment += (%this.grindReversed ? -1 : 1);
		if(%this.grindCurrSegment < 0 || %this.grindCurrSegment > %db.gGetSegmentCount())
		{
			pDebug("GRIND : Exiting grind; segment increment overflowed (" @ %this @ ", " @ %obj @ ")", %this, %obj);
			pDebug("   -Segment:" SPC %p @ "->" @ %this.grindCurrSegment, %this, %obj);

			if(!isObject(%this.tryGrindTransfer(%obj)))
				%this.grindExit(%obj);

			return;
		}
	}

	pDebug("   -Rate:" SPC %rate, %this, %obj);

	%this.grindLastRailPoint = VectorSub(%expectedPos, $Platformer::Rails::PlayerFixVector);

	%this.grindSchedule = %this.scheduleNoQuota(%rate, grindStep, %obj);
}

function Player::grindExit(%this, %obj)
{
	if(isEventPending(%this.grindSchedule))
		cancel(%this.grindSchedule);

	pDebug("GRIND : Player exiting grind (" @ $Sim::Time @ ", " @ %this @ ", " @ %obj @ ")", %this, %obj);

	%this.grinding = false;
	%this.grindObj = "";
	%this.grindPointLast = "";
	%this.grindCurrSegment = "";
	%this.grindSchedule = "";
	%this.grindReversed = "";
	%this.stopAudio(3);

	if(!isObject(%obj))
		return;

	%db = %obj.getDatablock();
	if(%db.grindExitVelocity !$= "")
	{
		if(%db.grindExitVelocityOverride)
		{
			pDebug("GRIND : Applying exit override velocity (" @ %this @ ", " @ %obj @ ")", %this, %obj);
			%this.setVelocity(%db.grindExitVelocity);
		}
		else
		{
			pDebug("GRIND : Applying exit velocity (" @ %this @ ", " @ %obj @ ")", %this, %obj);
			%this.addVelocity(%db.grindExitVelocity);
		}
		pDebug("   -Velocity:" SPC %db.grindExitVelocity, %this, %obj);
	}
}

function Player::updateGrindGhost(%this)
{
	if(isEventPending(%this.grindGhostSchedule))
		cancel(%this.grindGhostSchedule);

	if(!isObject(%shape = %this.grindGhostShape))
		return;

	if(!isObject(%obj = %shape.brick))
	{
		%shape.delete();
		return;
	}

	%shape.currPos += $Platformer::Rails::GhostShapeInc;
	%pos = %obj.gGetRailPoint(%shape.currPos, 1);
	%next = %obj.gGetRailPoint(mFloor(%shape.currPos)+1, 1);
	if(getWordCount(%next) != 3)
	{
		%shape.currPos = 0;
		%pos = %obj.gGetRailPoint(0, 1);
		%next = %obj.gGetRailPoint(1, 1);
	}

	%shape.setTransform(%pos SPC %shape.getPointAtRotation(%next));

	%this.grindGhostSchedule = %this.schedule($Platformer::Rails::GhostShapeUpdate, updateGrindGhost);
}

function Player::initiateGrindGhost(%player, %obj)
{
	pDebug("GRINDGHOST : New arrow (" @ %obj @ ", " @ %player @ ")", %obj, %player);

	%start = %obj.gGetRailPoint(0, 1);
	%next = %obj.gGetRailPoint(1, 1);

	if(!isObject(%player.grindGhostShape))
	{
		%player.grindGhostShape = new StaticShape("GhostShape_" @ %player.client.getBLID())
								{
									datablock = arrowIndicatorShape;

									client = %client;
									brick = %obj;

									currPos = 0;
								};
		%player.grindGhostShape.setScale("0.5 0.5 0.5");
		%player.grindGhostShape.setNodeColor("ALL", "1 1 0 1");
		MissionCleanup.add(%player.grindGhostShape);
	}
	%obj.grindGhostShape = %player.grindGhostShape;

	pDebug("   -Shape:" SPC %player.grindGhostShape, %obj, %player);

	%player.grindGhostShape.setTransform(%start SPC %player.grindGhostShape.getPointAtRotation(%next));

	%player.grindGhostSchedule = %player.schedule($Platformer::Rails::GhostShapeUpdate, updateGrindGhost);
}

function fxDTSBrick::beginGrindGhost(%obj)
{
	%this = %obj.getDatablock();
	if(!%this.isValidRail() || %obj.isPlanted)
		return;

	%group = %obj.getGroup();
	if(!isObject(%group))
		return;

	%client = %group.client;
	if(!isObject(%client))
		return;

	%player = %client.player;
	if(!isObject(%player))
		return;

	%player.schedule(0, initiateGrindGhost, %obj);
}

package Platformer_GrindRails
{
	function fxDTSBrickData::onAdd(%this, %obj)
	{
		parent::onAdd(%this, %obj);

		if(!%this.isValidRail() || %obj.isPlanted)
			return;

		%obj.schedule(0, beginGrindGhost);
	}

	function fxDTSBrickData::onRemove(%this, %obj)
	{
		parent::onRemove(%this, %obj);

		if(%obj.isPlanted)
			return;

		if(isObject(%shape = %obj.grindGhostShape))
		{
			if(isObject(%player = %shape.client.player))
				cancel(%shape.client.player.grindGhostSchedule);

			pDebug("GRINDGHOST : Destroy arrow (" @ %obj @ ", " @ %player @ ")", %obj, %player);
			pDebug("   -Shape:" SPC %obj.grindGhostShape, %obj, %player);

			%shape.delete();
		}
	}

	function fxDTSBrick::setDatablock(%obj, %db)
	{
		%odb = %obj.getDatablock();
		parent::setDatablock(%obj, %db);

		if(%obj.isPlanted)
			return;

		%r1 = %odb.isValidRail();
		%r2 = %db.isValidRail();

		if(%r1 && !%r2)
		{
			if(isObject(%shape = %obj.grindGhostShape))
			{
				if(isObject(%player = %shape.client.player))
					cancel(%shape.client.player.grindGhostSchedule);

				pDebug("GRINDGHOST : Destroy arrow (" @ %obj @ ", " @ %player @ ")", %obj, %player);
				pDebug("   -Shape:" SPC %obj.grindGhostShape, %obj, %player);

				%shape.delete();
			}
		}
		else if(!%r1 && %r2)
			%obj.schedule(0, beginGrindGhost);
	}

	function fxDTSBrickData::onPlayerTouch(%this, %obj, %player)
	{
		parent::onPlayerTouch(%this, %obj, %player);

		if(!%this.isValidRail() || $Sim::Time - %player.railTimeout < $Platformer::Rails::TouchTimeout || %player.grinding)
			return;

		pDebug("GRIND : Player touched rail (" @ %player @ ", " @ %obj @ ")", %obj, %player);

		%player.railTimeout = $Sim::Time;

		%player.grindEnter(%obj);
	}

	function serverCmdShiftBrick(%this, %x, %y, %z)
	{
		parent::serverCmdShiftBrick(%this, %x, %y, %z);

		if(isObject(%this.player.grindGhostShape))
			%this.player.updateGrindGhost();
	}

	function serverCmdSuperShiftBrick(%this, %x, %y, %z)
	{
		parent::serverCmdSuperShiftBrick(%this, %x, %y, %z);

		if(isObject(%this.player.grindGhostShape))
			%this.player.updateGrindGhost();
	}

	function serverCmdRotateBrick(%this, %val)
	{
		parent::serverCmdRotateBrick(%this, %val);

		if(isObject(%this.player.grindGhostShape))
			%this.player.updateGrindGhost();
	}

	function Armor::onTrigger(%this, %obj, %slot, %val)
	{
		parent::onTrigger(%this, %obj, %slot, %val);

		if(!%obj.grinding || !%val)
			return;

		switch(%slot)
		{
			case 2:
				%rail = %obj.grindObj;
				%railDB = %rail.getDatablock();
				%obj.grindExit();

				%fwdDir = %obj.getForwardVector();
				%xyVel = VectorScale(%fwdDir, (%railDB.railExitSpeed $= "" ? $Platformer::Rails::DefaultExitSpeed : %railDB.railExitSpeed));
				%baseVel = VectorAdd(%xyVel, "0 0" SPC (%railDB.railJumpSpeed $= "" ? $Platformer::Rails::DefaultJumpSpeed : %railDB.railJumpSpeed));

				%vel = VectorAdd(%baseVel, %railDB.railJumpMod);

				%obj.addVelocity(%vel);

			case 0 or 4:
				%rail = %obj.grindObj;
				%db = %rail.getDatablock();
				// %currPos = %rail.gGetRailPoint(%obj.grindPointLast, 0, %obj.grindReversed);
				%currPos = %obj.grindLastRailPoint;

				pDebug("GRINDSWITCH : Rail switch (" @ %obj @ ", " @ %rail @ ")", %obj, %rail);
				pDebug("   currPos:" SPC %currPos, %obj, %rail);

				%r = false;
				%comp = mFloatLength(VectorDot(%obj.getVelocity(), %obj.getForwardVector()), 0);
				if(%comp < 0)
					%r = true;

				pDebug("   comp:" SPC %comp, %obj, %rail);

				%angleID = (%rail.getAngleID() + (%obj.grindReversed ? 2 : 0)) % 4;
				pDebug("   angleID:" SPC %rail.getAngleID(), %obj, %rail);
				%angle = %angleID - ((%r ? -1 : 1) * (%slot == 4 ? -1 : 1));
				pDebug("   angle:" SPC %angle, %obj, %rail);

				%ray = RelayCheck(%currPos, %angle, $Platformer::Rails::MaxSwitchDist, "0 -1 0", 1);

				%new = firstWord(%ray);
				if(!isObject(%new))
					return;

				%newdb = %new.getDatablock();
				if(!%newdb.isValidRail())
					return;

				pDebug("   -New:" SPC %new, %obj, %rail, %new);

				%hitPos = getWords(%ray, 1, 3);
				%newStart = %new.gGetRailPoint(0);
				%len = %newdb.gGetTotalLength();
				%dist = VectorDist(%hitPos, %newStart);
				%startPoint = %dist / %len;

				pDebug("   -StartPoint:" @ %startPoint, %obj, %rail, %new);

				%obj.grindTransfer(%rail, %new, %startPoint);
		}
	}
};
activatePackage(Platformer_GrindRails);
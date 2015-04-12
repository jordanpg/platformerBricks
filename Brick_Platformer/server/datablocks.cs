datablock AudioProfile(RailGrindSound)
{
	description = "AudioClosestLooping3D";
	fileName = $Platformer::Assets @ "grind.wav";
	preload = true;
};

datablock AudioProfile(RailContactSound)
{
	description = "AudioClosest3D";
	fileName = $Platformer::Assets @ "contact.wav";
	preload = true;
};

datablock AudioProfile(BumperSound)
{
	description = "AudioClosest3D";
	fileName = $Platformer::Assets @ "bumper.wav";
	preload = true;
};

datablock AudioProfile(BumperFarSound)
{
	description = "AudioClosest3D";
	fileName = $Platformer::Assets @ "bumper2.wav";
	preload = true;
};

datablock ParticleData(GrindSparkParticle)
{
	textureName          	= "base/data/particles/star1";
	dragCoefficient      	= 1;
	gravityCoefficient   	= 1.5;
	inheritedVelFactor   	= 0.2;
	constantAcceleration 	= 0.0;
	lifetimeMS           	= 1500;
	lifetimeVarianceMS   	= 150;
	spinSpeed				= 10.0;
	spinRandomMin			= -500.0;
	spinRandomMax			= 500.0;
	colors[0]     			= "0.9 0.4 0.0 0.9";
	colors[1]     			= "0.9 0.5 0.0 0.0";
	sizes[0]      			= 0.2;
	sizes[1]      			= 0.0;

	useInvAlpha 			= false;
};

datablock ParticleEmitterData(GrindSparkEmitter)
{
	particles				= "GrindSparkParticle";
	lifetimeMS				= 0;
	ejectionPeriodMS		= 20;
	periodVarianceMS		= 0;
	ejectionVelocity		= 5;
	thetaMin 				= 15;
	thetaMax 				= 90;
	phiReferenceVel 		= 0;
	phiVariance 			= 360;
	overrideAdvance 		= false;
	useEmitterColors 		= false;
	orientParticles 		= false;

	uiName = "Grind Spark";
};

datablock StaticShapeData(arrowIndicatorShape)
{
	shapeFile = $Platformer::Assets @ "arrow.dts";
};



//Bumpers
datablock fxDTSBrickData(Brick2x2BumperData)
{
	brickFile = $Platformer::Assets @ "bumper2x2.blb";
	iconName = $Platformer::Assets @ "icon_bumper2x2";

	category = "Special";
	subCategory = "Platformer - Misc";
	uiName = "2x2 Bumper";

	platformerType = "Bumper";
	bumperPower = 25; //Power of the bumper. Used only in bumper/bumperdir.
	bumerAddSpeed = 0; //Don't mess with this, it doesn't do anything good.
	bumperDirectional = false; //Flag for directional bumpers.
};

datablock fxDTSBrickData(Brick4x4BumperData)
{
	brickFile = $Platformer::Assets @ "bumper4x4.blb";
	iconName = $Platformer::Assets @ "icon_bumper4x4";

	category = "Special";
	subCategory = "Platformer - Misc";
	uiName = "4x4 Bumper";

	platformerType = "Bumper";
	bumperPower = 25;
	bumerAddSpeed = 0;
	bumperDirectional = false;
};

datablock fxDTSBrickData(Brick2x2BumperSideData : Brick2x2BumperData)
{
	brickFile = $Platformer::Assets @ "bumper2x2Side.blb";
	iconName = $Platformer::Assets @ "icon_bumper2x2Side";

	uiName = "2x2 Sideways Bumper";

	bumperDirectional = true;
};

datablock fxDTSBrickData(Brick4x4BumperSideData : Brick4x4BumperData)
{
	brickFile = $Platformer::Assets @ "bumper4x4Side.blb";
	iconName = $Platformer::Assets @ "icon_bumper4x4Side";

	uiName = "4x4 Sideways Bumper";

	bumperDirectional = true;
};



//Grind Rails
datablock fxDTSBrickData(brick1x16fRailData : brick1x16fData)
{
	uiName = "1x16f Grind Rail";
	
	category = "Special";
	subCategory = "Grind Rails";

	platformerType = "Rail";

	railPoint0 = "0 3.75 0";
	railPoint1 = "0 -3.75 0";

	railEndSearchDir = "0 -1 0";
	railStartSearchDir = "0 1 0";
};

datablock fxDTSBrickData(brick1x8fRailData : brick1x8fData)
{
	uiName = "1x8f Grind Rail";
	
	category = "Special";
	subCategory = "Grind Rails";

	platformerType = "Rail";

	railPoint0 = "0 1.75 0";
	railPoint1 = "0 -1.75 0";

	railEndSearchDir = "0 -1 0";
	railStartSearchDir = "0 1 0";
};

datablock fxDTSBrickData(brick2x2fCornerRailData)
{
	brickFile = $Platformer::Assets @ "2x2Fcorner.blb";
	iconName = $Platformer::Assets @ "2x2F Corner";

	uiName = "2x2f Corner Grind Rail";

	category = "Special";
	subCategory = "Grind Rails";

	platformerType = "Rail";

	railPoint0 = "0.25 -0.25 0";
	railPoint1 = "-0.25 -0.25 0";
	railPoint2 = "-0.25 0.25 0";

	railEndSearchDir = "0 1 0";
	railStartSearchDir = "1 0 0";

	railUpdateOnTransfer = false;

	railUpdateRateMS = 25;

	railInterpolationMethodPoint = 1;
	railInterpolationMethodVel = 1;

	railEnforcePosition = true;
};

datablock fxDTSBrickData(brick1x16fRailHighSpeedData : brick1x16fRailData)
{
	uiName = "1x16f Grind Rail (High Speed)";

	railSpeed0 = $Platformer::Rails::DefaultSpeed * 1.5;
};

datablock fxDTSBrickData(brick1x16fRailHighSpeedTransitionData : brick1x16fRailData)
{
	uiName = "1x16f Grind Rail (Low->High Speed)";

	railPoint1 = "0 0 0";
	railSpeed1 = $Platformer::Rails::DefaultSpeed * 1.5;

	railPoint2 = "0 -3.75 0";

	railInterpolationMethodVel = 1;
	railInterpolationMethodPoint = 0;

	// railUpdateRateMS = 25;
};
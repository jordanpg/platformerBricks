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

datablock AudioProfile(BoosterSound)
{
	description = "AudioClosest3D";
	fileName = $Platformer::Assets @ "booster.wav";
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

//Booster
datablock fxDTSBrickData(BrickBoosterData)
{
	brickFile = $Platformer::Assets @ "booster.blb";
	iconName = $Platformer::Assets @ "icon_booster";

	category = "Special";
	subCategory = "Platformer - Misc";
	uiName = "Booster";

	platformerType = "Booster";
	boosterPower = 25;
};

//Bumpers
datablock fxDTSBrickData(BrickBumperData)
{
	brickFile = $Platformer::Assets @ "Bumper.blb";
	iconName = $Platformer::Assets @ "Bumper";

	category = "Special";
	subCategory = "Platformer - Misc";
	uiName = "Bumper";

	platformerType = "Bumper";
	bumperPower = 25; //Power of the bumper. Used only in bumper/bumperdir.
	bumperDirectional = false; //Flag for directional bumpers.
};

datablock fxDTSBrickData(BrickBumperDirData : BrickBumperData)
{
	brickFile = $Platformer::Assets @ "BumperUp.blb";
	iconName = $Platformer::Assets @ "BumperUp";
	
	uiName = "Sideways Bumper";

	bumperDirectional = true;
};

//Grind Rails
datablock fxDTSBrickData(brick1x16fRailData : brick1x16fData)
{
	uiName = "1x16f Rail";
	
	category = "Special";
	subCategory = "Grind Rails";

	platformerType = "Rail";

	railPoint0 = "0 3.75 0";
	railPoint1 = "0 -3.75 0";

	railEndSearchDir = "0 -1 0";
	railStartSearchDir = "0 1 0";
};

datablock fxDTSBrickData(brick1x12fRailData : brick1x12fData)
{
	uiName = "1x12f Rail";
	
	category = "Special";
	subCategory = "Grind Rails";

	platformerType = "Rail";

	railPoint0 = "0 2.75 0";
	railPoint1 = "0 -2.75 0";

	railEndSearchDir = "0 -1 0";
	railStartSearchDir = "0 1 0";
};

datablock fxDTSBrickData(brick1x10fRailData : brick1x10fData)
{
	uiName = "1x10f Rail";
	
	category = "Special";
	subCategory = "Grind Rails";

	platformerType = "Rail";

	railPoint0 = "0 2.25 0";
	railPoint1 = "0 -2.25 0";

	railEndSearchDir = "0 -1 0";
	railStartSearchDir = "0 1 0";
};

datablock fxDTSBrickData(brick1x8fRailData : brick1x8fData)
{
	uiName = "1x8f Rail";
	
	category = "Special";
	subCategory = "Grind Rails";

	platformerType = "Rail";

	railPoint0 = "0 1.75 0";
	railPoint1 = "0 -1.75 0";

	railEndSearchDir = "0 -1 0";
	railStartSearchDir = "0 1 0";
};

datablock fxDTSBrickData(brick1x6fRailData : brick1x6fData)
{
	uiName = "1x6f Rail";
	
	category = "Special";
	subCategory = "Grind Rails";

	platformerType = "Rail";

	railPoint0 = "0 1.25 0";
	railPoint1 = "0 -1.25 0";

	railEndSearchDir = "0 -1 0";
	railStartSearchDir = "0 1 0";
};

datablock fxDTSBrickData(brick1x4fRailData : brick1x4fData)
{
	uiName = "1x4f Rail";
	
	category = "Special";
	subCategory = "Grind Rails";

	platformerType = "Rail";

	railPoint0 = "0 0.75 0";
	railPoint1 = "0 -0.75 0";

	railEndSearchDir = "0 -1 0";
	railStartSearchDir = "0 1 0";
};

datablock fxDTSBrickData(brick1x2fRailData : brick1x2fData)
{
	uiName = "1x2f Rail";
	
	category = "Special";
	subCategory = "Grind Rails";

	platformerType = "Rail";

	railPoint0 = "0 0.25 0";
	railPoint1 = "0 -0.25 0";

	railEndSearchDir = "0 -1 0";
	railStartSearchDir = "0 1 0";
};

datablock fxDTSBrickData(brick2x2fCornerRailData)
{
	brickFile = $Platformer::Assets @ "2x2Fcorner.blb";
	iconName = $Platformer::Assets @ "2x2F Corner";

	uiName = "2x2f Corner Rail";

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

datablock fxDTSBrickData(brick2x2fCornerRailReverseData)
{
	brickFile = $Platformer::Assets @ "2x2Fcorner.blb";
	iconName = $Platformer::Assets @ "2x2F Corner";

	uiName = "2x2f Corner Inverse Rail";

	category = "Special";
	subCategory = "Grind Rails";

	platformerType = "Rail";

	railPoint2 = "0.25 -0.25 0";
	railPoint1 = "-0.25 -0.25 0";
	railPoint0 = "-0.25 0.25 0";

	railEndSearchDir = "0 1 0";
	railStartSearchDir = "1 0 0";

	railUpdateOnTransfer = false;

	railUpdateRateMS = 25;

	railInterpolationMethodPoint = 1;
	railInterpolationMethodVel = 1;

	railEnforcePosition = true;
};

//REPETITIVE CODE HOORAY!!

datablock fxDTSBrickData(brick1x16fRailFastData : brick1x16fRailData)
{
	subCategory = "Grind Rails Fast";

	uiName = "1x16f Fast Rail";

	railSpeed0 = $Platformer::Rails::DefaultSpeed * 1.5;
};

datablock fxDTSBrickData(brick1x12fRailFastData : brick1x12fRailData)
{
	subCategory = "Grind Rails Fast";

	uiName = "1x12f Fast Rail";

	railSpeed0 = $Platformer::Rails::DefaultSpeed * 1.5;
};

datablock fxDTSBrickData(brick1x10fRailFastData : brick1x10fRailData)
{
	subCategory = "Grind Rails Fast";

	uiName = "1x10f Fast Rail";

	railSpeed0 = $Platformer::Rails::DefaultSpeed * 1.5;
};

datablock fxDTSBrickData(brick1x8fRailFastData : brick1x8fRailData)
{
	subCategory = "Grind Rails Fast";

	uiName = "1x8f Fast Rail";

	railSpeed0 = $Platformer::Rails::DefaultSpeed * 1.5;
};

datablock fxDTSBrickData(brick1x6fRailFastData : brick1x6fRailData)
{
	subCategory = "Grind Rails Fast";

	uiName = "1x6f Fast Rail";

	railSpeed0 = $Platformer::Rails::DefaultSpeed * 1.5;
};

datablock fxDTSBrickData(brick1x4fRailFastData : brick1x4fRailData)
{
	subCategory = "Grind Rails Fast";

	uiName = "1x4f Fast Rail";

	railSpeed0 = $Platformer::Rails::DefaultSpeed * 1.5;
};

datablock fxDTSBrickData(brick1x2fRailFastData : brick1x2fRailData)
{
	subCategory = "Grind Rails Fast";

	uiName = "1x2f Fast Rail";

	railSpeed0 = $Platformer::Rails::DefaultSpeed * 1.5;
};

datablock fxDTSBrickData(brick1x16fRailSpeedTransitionData : brick1x16fRailFastData)
{
	subCategory = "Grind Rails Accel.";

	uiName = "1x16f Slow->Fast Rail";

	railPoint1 = "0 0 0";
	railSpeed1 = $Platformer::Rails::DefaultSpeed * 1.5;

	railPoint2 = "0 -3.75 0";

	railInterpolationMethodVel = 1;
	railInterpolationMethodPoint = 0;

	// railUpdateRateMS = 25;
};

datablock fxDTSBrickData(brick1x12fRailSpeedTransitionData : brick1x12fRailFastData)
{
	subCategory = "Grind Rails Accel.";

	uiName = "1x12f Slow->Fast Rail";

	railPoint1 = "0 0 0";
	railSpeed1 = $Platformer::Rails::DefaultSpeed * 1.5;

	railPoint2 = "0 -2.75 0";

	railInterpolationMethodVel = 1;
	railInterpolationMethodPoint = 0;

	// railUpdateRateMS = 25;
};

datablock fxDTSBrickData(brick1x10fRailSpeedTransitionData : brick1x10fRailFastData)
{
	subCategory = "Grind Rails Accel.";
	
	uiName = "1x10f Slow->Fast Rail";

	railPoint1 = "0 0 0";
	railSpeed1 = $Platformer::Rails::DefaultSpeed * 1.5;

	railPoint2 = "0 -2.25 0";

	railInterpolationMethodVel = 1;
	railInterpolationMethodPoint = 0;

	// railUpdateRateMS = 25;
};

datablock fxDTSBrickData(brick1x8fRailSpeedTransitionData : brick1x8fRailFastData)
{
	subCategory = "Grind Rails Accel.";
	
	uiName = "1x8f Slow->Fast Rail";

	railPoint1 = "0 0 0";
	railSpeed1 = $Platformer::Rails::DefaultSpeed * 1.5;

	railPoint2 = "0 -1.75 0";

	railInterpolationMethodVel = 1;
	railInterpolationMethodPoint = 0;

	// railUpdateRateMS = 25;
};

datablock fxDTSBrickData(brick1x6fRailSpeedTransitionData : brick1x6fRailFastData)
{
	subCategory = "Grind Rails Accel.";
	
	uiName = "1x6f Slow->Fast Rail";

	railPoint1 = "0 0 0";
	railSpeed1 = $Platformer::Rails::DefaultSpeed * 1.5;

	railPoint2 = "0 -1.25 0";

	railInterpolationMethodVel = 1;
	railInterpolationMethodPoint = 0;

	// railUpdateRateMS = 25;
};

datablock fxDTSBrickData(brick1x4fRailSpeedTransitionData : brick1x4fRailFastData)
{
	subCategory = "Grind Rails Accel.";
	
	uiName = "1x4f Slow->Fast Rail";

	railPoint1 = "0 0 0";
	railSpeed1 = $Platformer::Rails::DefaultSpeed * 1.5;

	railPoint2 = "0 -0.75 0";

	railInterpolationMethodVel = 1;
	railInterpolationMethodPoint = 0;

	// railUpdateRateMS = 25;
};

datablock fxDTSBrickData(brick1x2fRailSpeedTransitionData : brick1x2fRailFastData)
{
	subCategory = "Grind Rails Accel.";
	
	uiName = "1x2f Slow->Fast Rail";

	railPoint1 = "0 0 0";
	railSpeed1 = $Platformer::Rails::DefaultSpeed * 1.5;

	railPoint2 = "0 -0.25 0";

	railInterpolationMethodVel = 1;
	railInterpolationMethodPoint = 0;

	// railUpdateRateMS = 25;
};
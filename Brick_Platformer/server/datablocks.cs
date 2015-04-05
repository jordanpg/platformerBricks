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
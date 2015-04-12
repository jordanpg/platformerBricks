function pDebug(%msg, %o0, %o1, %o2, %o3, %o4, %o5)
{
	%highest = 0;
	for(%i = 0; %i < 6; %i++)
	{
		if(!isObject(%o[%i]))
			continue;

		if(%o[%i].pDebugCondition !$= "")
			if(!eval("return" SPC %o[%i].pDebugCondition @ ";")) continue;

		if(%o[%i].pDebug > %highest)
			%highest = %highest | %o[%i].pDebug;
	}

	switch($pDebug | %highest)
	{
		case 0: return;
		case 1: echo(%msg);
		case 2:	talk(%msg);
		case 3: 
			echo(%msg);
			talk(%msg);
		default:
			warn(%msg);
	}
}

function eulerToAxis(%euler)
{
	%euler = VectorScale(%euler,$pi / 180);
	%matrix = MatrixCreateFromEuler(%euler);
	return getWords(%matrix,3,6);
}

function axisToEuler(%axis)
{
	%angleOver2 = getWord(%axis,3) * 0.5;
	%angleOver2 = -%angleOver2;
	%sinThetaOver2 = mSin(%angleOver2);
	%cosThetaOver2 = mCos(%angleOver2);
	%q0 = %cosThetaOver2;
	%q1 = getWord(%axis,0) * %sinThetaOver2;
	%q2 = getWord(%axis,1) * %sinThetaOver2;
	%q3 = getWord(%axis,2) * %sinThetaOver2;
	%q0q0 = %q0 * %q0;
	%q1q2 = %q1 * %q2;
	%q0q3 = %q0 * %q3;
	%q1q3 = %q1 * %q3; //Secrets are hard to come by, cherish them while they last.
	%q0q2 = %q0 * %q2;
	%q2q2 = %q2 * %q2;
	%q2q3 = %q2 * %q3;
	%q0q1 = %q0 * %q1;
	%q3q3 = %q3 * %q3;
	%m13 = 2.0 * (%q1q3 - %q0q2);
	%m21 = 2.0 * (%q1q2 - %q0q3);
	%m22 = 2.0 * %q0q0 - 1.0 + 2.0 * %q2q2;
	%m23 = 2.0 * (%q2q3 + %q0q1);
	%m33 = 2.0 * %q0q0 - 1.0 + 2.0 * %q3q3;
	return mRadToDeg(mAsin(%m23)) SPC mRadToDeg(mAtan(-%m13, %m33)) SPC mRadToDeg(mAtan(-%m21, %m22));
}

function VectorInterpolate_Linear(%a, %b, %mu)
{
	return VectorAdd(VectorScale(%a, 1-%mu), VectorScale(%b, %mu));
}

function VectorInterpolate_Cosine(%a, %b, %mu)
{
	// talk("cosine" SPC %a SPC %b SPC %mu);
	%mu2 = (1 - mCos(%mu * $pi)) / 2;
	return VectorAdd(VectorScale(%a, 1-%mu2), VectorScale(%b, %mu2));
}

function rotateVector(%vector, %axis, %val) //Rotates a vector around the axis by an angleID.
{
	if(%val < 0)
		%val += 4;
	if(%val > 3)
		%val -= 4;
	switch(%val)
	{
		case 1:
			%nX = getWord(%axis, 0) + (getWord(%vector, 1) - getWord(%axis, 1));
			%nY = getWord(%axis, 1) - (getWord(%vector, 0) - getWord(%axis, 0));
			%new = %nX SPC %nY SPC getWord(%vector, 2);
		case 2:
			%nX = getWord(%axis, 0) - (getWord(%vector, 0) - getWord(%axis, 0));
			%nY = getWord(%axis, 1) - (getWord(%vector, 1) - getWord(%axis, 1));
			%new = %nX SPC %nY SPC getWord(%vector, 2);
		case 3:
			%nX = getWord(%axis, 0) - (getWord(%vector, 1) - getWord(%axis, 1));
			%nY = getWord(%axis, 1) + (getWord(%vector, 0) - getWord(%axis, 0));
			%new = %nx SPC %nY SPC getWord(%vector, 2);
		default: %new = vectorAdd(%vector, %axis);
	}
	return %new;
}

function RelayCheck(%start, %angle, %len, %xyz, %returnRay) //Essentially allows for more dynamic relay-like events with bricks.
{
	%angle %= 4;

	pDebug("   [RelayCheck]-Angle:" SPC %angle);

	if(%len $= "")
		%len = 0.5; //one stud
	if(getWordCount(%xyz) == 3)
		%vect = VectorScale(%xyz, %len);
	else
		%vect = %len SPC "0 0";

	pDebug("   [RelayCheck]-Len:" SPC %len);
	pDebug("   [RelayCheck]-Vect:" SPC %vect);

	%end = rotateVector(%vect, "0 0 0", %angle);
	pDebug("   [RelayCheck]-Start:" SPC %start);
	pDebug("   [RelayCheck]-End_I:" SPC %end);
	%end = VectorAdd(%end, %start);
	pDebug("   [RelayCheck]-End:" SPC %end);
	// echo(%end);
	%cast = containerRaycast(%start, %end, $TypeMasks::FxBrickObjectType);
	pDebug("   [RelayCheck]-Cast:" SPC %cast);
	// echo(%cast);
	%obj = firstWord(%cast);
	if(isObject(%obj) && %obj.getClassName() $= "fxDTSBrick")
		return (%returnRay ? %cast : %obj);
	return -1;
}

function ShapeBase::getEulerRotation(%this)
{
	%rot = getWords(%this.getTransform(), 3, 6);
	return axisToEuler(%rot);
}

function ShapeBase::PointAt(%this, %pos)
{
	%thispos = %this.getPosition();
	%delta = VectorSub(%pos, %thispos);
	%dX = getWord(%delta, 0);
	%dY = getWord(%delta, 1);
	%dZ = getWord(%delta, 2);
	%hyp = VectorLen(%dX SPC %dY SPC 0);

	%rotZ = mAtan(%dX, %dY) * -1;
	%rotX = mAtan(%dZ, %hyp);

	%this.setTransform(%thispos SPC eulerRadToMatrix(%rotX SPC 0 SPC %rotZ));
}

function ShapeBase::getPointAtRotation(%this, %pos)
{
	%thispos = %this.getPosition();
	%delta = VectorSub(%pos, %thispos);
	%dX = getWord(%delta, 0);
	%dY = getWord(%delta, 1);
	%dZ = getWord(%delta, 2);
	%hyp = VectorLen(%dX SPC %dY SPC 0);

	%rotZ = mAtan(%dX, %dY) * -1;
	%rotX = mAtan(%dZ, %hyp);

	return eulerRadToMatrix(%rotX SPC 0 SPC %rotZ);
}

function SimGroup::hasNTObject(%bg, %nt) //Fairly-self-explanatory, returns an index if the brick name is found in a brickgroup.
{
	for(%i = 0; %i < %bg.NTNameCount; %i++)
	{
		if(%bg.NTName[%i] $= %nt)
			return %i;
	}
	return -1;
}

function ShapeBase::getObjectBoxSize(%this)
{
	%box = %this.getObjectBox();
	%min = getWords(%box, 0, 2);
	%max = getWords(%box, 3, 5);

	%base = VectorSub(%max, %min);

	%scale = %this.getScale();

	for(%i = 0; %i < 3; %i++)
		%size = %size SPC getWord(%base, %i) * getWord(%scale, %i);

	return ltrim(%size);
}
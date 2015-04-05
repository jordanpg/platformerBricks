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

function vectorFloor(%vec) //Does mFloor on each axis of a vector.
{
	for(%i = 0; %i < 3; %i++)
	{
		%w = getWord(%vec, %i);
		%vec = setWord(%vec, %i, mFloor(%w));
	}
	return %vec;
}

function GrindFixVector(%vec) //Modifies vectors so that each axis is either 1 or 0.
{
	for(%i = 0; %i < 3; %i++)
	{
		%w = getWord(%vec, %i);
		switch(%i)
		{
			case 0: %vec1 = VectorNormalize(%w SPC "0 0");
			case 1: %vec2 = VectorNormalize("0" SPC %w SPC "0");
			case 2: %vec3 = VectorNormalize("0 0" SPC %w);
		}
	}
	%vec = VectorAdd(%vec3, VectorAdd(%vec1, %vec2));
	return %vec;
}

function GrindDirCompare(%bDir, %pVel) //Figures out if directions of a brick and a player's velocity are opposite, the same, or unmatching. Expects GrindFixVector to be used on player velocity.
{
	if(getWord(%bDir, 0) == 0)
	{
		%yB = getWord(%bDir, 1);
		%yP = getWord(%pVel, 1);
		if(%yB == 0)
			return -1;
		else if(%yP == 0)
			return -1;
		else if(%yB == %yP)
			return 1;
		else if(%yB == %yP * -1)
			return 0;
	}
	else
	{
		%xB = getWord(%bDir, 0);
		%xP = getWord(%pVel, 0);
		if(%xB == 0)
			return -1;
		else if(%xP == 0)
			return -1;
		else if(%xB == %xP)
			return 1;
		else if(%xB == %xP * -1)
			return 0;
	}
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
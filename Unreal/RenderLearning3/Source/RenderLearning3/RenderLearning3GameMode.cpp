// Copyright Epic Games, Inc. All Rights Reserved.

#include "RenderLearning3GameMode.h"
#include "RenderLearning3Character.h"
#include "UObject/ConstructorHelpers.h"

ARenderLearning3GameMode::ARenderLearning3GameMode()
{
	// set default pawn class to our Blueprinted character
	static ConstructorHelpers::FClassFinder<APawn> PlayerPawnBPClass(TEXT("/Game/ThirdPersonCPP/Blueprints/ThirdPersonCharacter"));
	if (PlayerPawnBPClass.Class != NULL)
	{
		DefaultPawnClass = PlayerPawnBPClass.Class;
	}
}

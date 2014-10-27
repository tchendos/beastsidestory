using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Constants
{
	public static int FIELD_SIZE_X;
	public static int FIELD_SIZE_Y;
	public static int FIELD_GRID_X;
	public static int FIELD_GRID_Y;

	public static Recti FIELD_RECT;
	public static int FIELD_POS_X;
	public static int FIELD_POS_Y;

	public static void Init()
	{
		FIELD_SIZE_X = GameManager.Instance.Definitions.FieldSizeX;
		FIELD_SIZE_Y = GameManager.Instance.Definitions.FieldSizeY;
		FIELD_GRID_X = GameManager.Instance.Definitions.FieldGridX;
		FIELD_GRID_Y = GameManager.Instance.Definitions.FieldGridY;
		
		FIELD_RECT = new Recti(-FIELD_GRID_X * FIELD_SIZE_X / 2 + GameManager.Instance.Definitions.FieldCenterX,
		                                           -FIELD_GRID_Y * FIELD_SIZE_Y / 2 + GameManager.Instance.Definitions.FieldCenterY,
		                                           FIELD_GRID_X * FIELD_SIZE_X / 2 + GameManager.Instance.Definitions.FieldCenterX, 
		                                           FIELD_GRID_Y * FIELD_SIZE_Y / 2 + GameManager.Instance.Definitions.FieldCenterY);
		FIELD_POS_X = -FIELD_GRID_X * FIELD_SIZE_X / 2 + FIELD_GRID_X / 2 + GameManager.Instance.Definitions.FieldCenterX;
		FIELD_POS_Y = FIELD_GRID_Y * FIELD_SIZE_Y / 2 - FIELD_GRID_Y / 2 + GameManager.Instance.Definitions.FieldCenterY;
	}

	// PlayerPrefs
	public static string LevelsFinished = "LevelsFinished";
}

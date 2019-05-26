#include "config.js"

var doc = app.activeDocument;
var trimImage = true;
var respectAspect = true;
var resizeImage = true;
var removeMatte = true;

// Resize options
var width = 96;
var height = 96;

// Color options in RGB
var minRed = 0.0;
var minGreen = 255.0;
var minBlue = 0.0;

var maxRed = 0.0;
var maxGreen = 255.0;
var maxBlue = 0.0;

try
{
    trimImage = config.trim;
    respectAspect = config.respectAspect;

    resizeImage = config.resize;
    width = config.width;
    height = config.height;

    removeMatte = config.removeMatte;
    fuzziness = config.fuzziness;
    minRed = config.red;
    minGreen = config.green;
    minBlue = config.blue;
    maxRed = config.red;
    maxGreen = config.green;
    maxBlue = config.blue;
}
catch(e)
{
    alert("Config file not found.");
}

ProcessIcon();

function ProcessIcon()
{
    if(removeMatte)
    {
        SelectByColorRange();
        if(IsSelected())
        {
            doc.selection.clear()
        }
    }

    if(trimImage)
    {
        doc.trim(TrimType.TRANSPARENT);
    }

    if(respectAspect)
    {
        var largerDimension = Math.max(doc.height.as('px'), doc.width.as('px'));
        doc.resizeCanvas(largerDimension, largerDimension, AnchorPosition.MIDDLECENTER);
    }

    if(resizeImage)
    {
        DoResize(width, height);
    }

    activeDocument.save();
    activeDocument.close();
}


function SelectByColorRange() 
{
    var idSelectByColorRange = charIDToTypeID("ClrR");
    var selectByColorOptions = new ActionDescriptor();
    var colorModel = charIDToTypeID("RGBC");
  
    // Set fuzziness value
    var idFzns = charIDToTypeID("Fzns");
    selectByColorOptions.putInteger(idFzns, 0);

    // Set min colors values for selection
    var idMnm = charIDToTypeID("Mnm ");
    var minColorValues = new ActionDescriptor();
    var red = charIDToTypeID("Rd  ");
    minColorValues.putDouble(red, minRed);
    var green = charIDToTypeID("Grn ");
    minColorValues.putDouble(green, minGreen);
    var blue = charIDToTypeID("Bl  ");
    minColorValues.putDouble(blue, minBlue);
    selectByColorOptions.putObject(idMnm, colorModel, minColorValues);

    // Set max colors values for selection
    var idMxm = charIDToTypeID("Mxm ");
    var maxColorValues = new ActionDescriptor();
    var red = charIDToTypeID("Rd  ");
    maxColorValues.putDouble(red, maxRed);
    var green = charIDToTypeID("Grn ");
    maxColorValues.putDouble(green, maxGreen);
    var blue = charIDToTypeID("Bl  ");
    maxColorValues.putDouble(blue, maxBlue);
    selectByColorOptions.putObject(idMxm, colorModel, maxColorValues);
  
    var idcolorModel = stringIDToTypeID("colorModel");
    selectByColorOptions.putInteger(idcolorModel, 0);
    executeAction(idSelectByColorRange, selectByColorOptions, DialogModes.NO);
}

function DoResize(width, height) 
{
    var userUnits = preferences.rulerUnits;

    if (preferences.rulerUnits != Units.PIXELS) 
    {
        preferences.rulerUnits = Units.PIXELS;
    }

    doc.resizeImage(UnitValue(width, "px"), UnitValue(height, "px"), 72, ResampleMethod.AUTOMATIC, 0);
    preferences.rulerUnits = userUnits;
}

function SaveAsPNG(saveFile) 
{
    var saveOptions = new PNGSaveOptions();
    saveOptions.compression = 9;
    saveOptions.interlaced = false;

    activeDocument.saveAs(saveFile, saveOptions, true, Extension.LOWERCASE);
}

function IsSelected() 
{ 
    var isSelected = false; 
    var state = doc.activeHistoryState; 
    doc.selection.deselect(); 
    if (state != doc.activeHistoryState) 
    { 
        isSelected = true; 
        doc.activeHistoryState = state; 
    } 
    return isSelected; 
}

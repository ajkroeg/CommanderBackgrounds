Commander Backgrounds is a mod for HBS's Battletech computer game. It does nothing on its own, but in conjunction with ModTek, provides two features for json modding.

## Commander Backgrounds
This mod ensures that `BackgroundDef` events (used when starting a new career) apply their results as any other event. They have at their fingertips the full power of a normal event json.

## Fire all pilots
Useful mainly for custom backgrounds, the MechWarrior_Fire event now supports a custom value `"*"`. In an event result:

```
"Results" : {
  "Scope" : "Company",
  "Actions" : [
    {
      "Type" : "MechWarrior_Fire",
      "value" : "*",
      "valueConstant" : null,
      "additionalValues" : null
    }
  ]
}
```

fires everyone except the commander.


## Example 


```JSON
   {
...
    "Processors": [
        {
            "$type": "ProcessDefinitionProcessorOptions",
            "Enabled": true,
            "Processes": {
                "Custom Agile Process": [
                    "Bug"
                ]
            },
            "ProcessMaps": {
                "Custom Agile Process": "Other Agile Process"
            },
            "SourceName": "Source",
            "TargetName": "Target",
            "UpdateProcessDetails": true
        }
    ]
...
}
```

## Example Full

```
{% include sampleConfig/ProcessDefinitionProcessor-Full.json %}
```

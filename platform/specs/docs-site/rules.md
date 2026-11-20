---
applyTo: "docs/content/docs/**/index.md"
---

# Documentation Standards

All documentation content **must stay in sync with the codebase**.  
Every page must accurately reflect the associated **data files** and **schemas**.  
Existing manually added content must be preserved. Reorganise or adapt it as needed, but do not remove it.

---

## Front Matter

Each documentation file **may include** the following properties:

- **dataFile**: Path to the generated documentation data file (stored in `docs/data/classes`)  
  Example: `reference.tools.fieldmappingtool.yaml`

- **schemaFile**: Path to the JSON schema file (stored in `docs/static/schema`)  
  Example: `schema.tools.fieldmappingtool.json`

> ⚠️ Documentation pages **must not** contain options, properties, or samples that do not exist in the corresponding `dataFile`.

---

## Documentation Structure

Documentation files should generally include these sections (in order).  
Manually added content should be placed into the most relevant section, or reorganised if necessary.

1. **Overview**  
   - Subsections: *How It Works*, *Use Cases*  

2. **Configuration Structure**  
   - Subsections: *Options*, *Sample*, *Defaults*, *Basic Examples*, *Complex Examples*  

3. **Common Scenarios**  
   - A list of common scenarios with supporting samples  

4. **Good Practices**  
   - Focus on maintainability, readability, and clarity  

5. **Troubleshooting**  
   - Common issues and their resolutions  

6. **Schema**  
   - Use the `{{< class-schema >}}` shortcode to render the schema from `schemaFile`  

---

## Rules

- **Options** → use `{{< class-options >}}` to render the options table from `dataFile`.  
- **Sample** → use `{{< class-sample sample="sample" >}}` to render the main sample from `dataFile`.  
- **Defaults** → use `{{< class-sample sample="defaults" >}}` to render defaults from `dataFile`.  
- **Basic Examples / Complex Examples** →  
  - Include generated samples based on the `dataFile`.  
  - Cross-reference the class object and its usage context.  

---

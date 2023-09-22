# SearchableComboBox: A Custom ComboBox Control

`SearchableComboBox` is an enhanced version of the standard ComboBox control, providing users with additional features that significantly improve its utility in modern applications. This control is designed to address common use cases where a regular ComboBox may fall short.

## Features:

1. **Search Functionality**: As the name suggests, `SearchableComboBox` offers an embedded search box. This search box allows users to quickly filter through the items in the dropdown, making it particularly useful for ComboBoxes with a large number of items.
2. **Custom Sorting**: The control supports custom sort comparers. This means you can define your own logic to sort the items in the dropdown.
3. **Placeholder Watermark**: A placeholder can be displayed inside the ComboBox when no item is selected, guiding the user on the expected input or simply for improved aesthetics.
4. **Enhanced UI Feedback**: The control provides visual feedback when no matches are found for a search term.

## How to use:

### 1. Include the Namespace:
First, make sure to reference the namespace where the `SearchableComboBox` resides.

```xml
xmlns:local="clr-namespace:SearchableComboBox"
```

### 2. Use the Control:

You can use the `SearchableComboBox` control just like you would a regular ComboBox, but with the added properties and features.

```xml
<local:SearchableComboBox Placeholder="Select an item..." IsSearchEnabled="True">
    <!-- ComboBox items go here -->
</local:SearchableComboBox>
```

### 3. Properties:

- **IsSearchEnabled**: A boolean property that controls whether the search functionality is enabled or not.
- **Placeholder**: A string property to set the watermark text when no item is selected.
- **CustomSort**: An `IComparer` property to provide custom sorting logic for the items in the dropdown.
- **SearchTerm**: A string property that reflects the current search term. This can be bound to if necessary.


## Notes:

- The control uses a `DebounceTimer` to ensure that filtering isn't too aggressive. This means there's a slight delay from when a user stops typing to when the filter is applied. This delay is set to 300 milliseconds by default.
- The control reverts the selection to the last selected item if a user's search yields no results.
- Debug lines are present in the code (controlled by the `debug` flag) for developers to understand the control's flow and operations. These can be removed or toggled off in production.

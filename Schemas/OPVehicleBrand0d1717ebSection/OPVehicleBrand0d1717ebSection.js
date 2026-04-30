define("OPVehicleBrand0d1717ebSection", ["ServiceHelper"], function(ServiceHelper) {
	return {
		entitySchemaName: "OPVehicleBrand",
		details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "insert",
				"parentName": "SeparateModeActionButtonsContainer",
				"propertyName": "items",
				"name": "ImportVehicleBrandButton",
				"index": 4,
				"values": {
					"itemType": BPMSoft.ViewItemType.BUTTON,
					"caption": {"bindTo": "Resources.Strings.ImportBrandsButtonCaption"},
					"click": {"bindTo": "onImportBrandButtonClick"},
				}
			},
			{
				"operation": "insert",
				"parentName": "SeparateModeActionButtonsContainer",
				"propertyName": "items",
				"name": "ImportVehicleButton",
				"index": 5,
				"values": {
					"itemType": BPMSoft.ViewItemType.BUTTON,
					"caption": {"bindTo": "Resources.Strings.ImportAllButtonCaption"},
					"click": {"bindTo": "onImportAllButtonClick"},
				}
			},
			
		]/**SCHEMA_DIFF*/,
		methods: {
		
			onImportBrandButtonClick: function()	{
				this.showBodyMask();				
				var serviceData = {};
				
				ServiceHelper.callService("OPVehicleService", "ImportBrands",
                    function(response) {
						this.hideBodyMask();
                        var result = response.ImportBrandsResult;

						if(result && result.isSuccess){
                        	this.showInformationDialog(result.value);
							this.reloadGridData();
						}

						else
							this.showInformationDialog(result.error.message);

                    }, serviceData, this);
			},

			onImportAllButtonClick: function()	{
				this.showBodyMask();				
				var serviceData = {};
				
				ServiceHelper.callService("OPVehicleService", "StartFullImportTask",
                    function(response) {
						this.hideBodyMask();
                        var result = response.StartFullImportTaskResult;

						if(result && result.isSuccess){
                        	this.showInformationDialog(result.value);
						}

						else
							this.showInformationDialog(result.error.message);

                    }, serviceData, this);
			},



		}
	};
});

define("OPVehicleBrand1Page", ["ServiceHelper"], function (ServiceHelper) {
	return {
		entitySchemaName: "OPVehicleBrand",
		attributes: {},
		modules: /**SCHEMA_MODULES*/{}/**SCHEMA_MODULES*/,
		details: /**SCHEMA_DETAILS*/{
			"Files": {
				"schemaName": "FileDetailV2",
				"entitySchemaName": "OPVehicleBrandFile",
				"filter": {
					"masterColumn": "Id",
					"detailColumn": "OPVehicleBrand"
				}
			},
			"OPVehicleModelDetail": {
				"schemaName": "OPSchema1cb27b9fDetail",
				"entitySchemaName": "OPVehicleModel",
				"filter": {
					"detailColumn": "OPBrand",
					"masterColumn": "Id"
				}
			}
		}/**SCHEMA_DETAILS*/,
		businessRules: /**SCHEMA_BUSINESS_RULES*/{}/**SCHEMA_BUSINESS_RULES*/,
		businessRulesMultiplyActions: /**SCHEMA_ANGULAR_BUSINESS_RULES*/{}/**SCHEMA_ANGULAR_BUSINESS_RULES*/,
		methods: {

			onEntityInitialized: function () {
				this.callParent(arguments);
				this.checkAndPromptModelImport();
			},

			checkAndPromptModelImport: function () {
				var isLoaded = this.get("OPIsModelsLoaded");
				if (isLoaded) {
					return;
				}

				var message = this.get("Resources.Strings.ImportModelsPrompt");
				this.showConfirmationDialog(message, function (returnCode) {
					if (returnCode === this.BPMSoft.MessageBoxButtons.YES.returnCode) {
						this.callImportModelsService();
					}
				}, ["yes", "no"]);
			},

			callImportModelsService: function () {

				this.showBodyMask();

				var serviceData = {
					brandId: this.get("Id"),
					externalBrandId: this.get("OPExternalId")
				};

				ServiceHelper.callService("OPVehicleModelService", "ImportModels",
					function (response) {
						this.hideBodyMask();

						var result = response.ImportModelsResult;

						if (result && result.isSuccess) {
							this.set("OPIsModelsLoaded", true);

							this.save({
								isSilent: true,
								callback: function () {
									this.hideBodyMask();
									this.showInformationDialog("Данные успешно загружены.");

									this.updateDetail({ detail: "OPVehicleModelDetail" });
								},
								scope: this
							});
						}

						else
							this.showInformationDialog(result.error.message);

					}, serviceData, this);
			}

		},
		dataModels: /**SCHEMA_DATA_MODELS*/{}/**SCHEMA_DATA_MODELS*/,
		diff: /**SCHEMA_DIFF*/[
			{
				"operation": "insert",
				"name": "OPName1a415e55-6a00-46d3-80e7-b9b889f4f9cf",
				"values": {
					"layout": {
						"colSpan": 24,
						"rowSpan": 1,
						"column": 0,
						"row": 0,
						"layoutName": "ProfileContainer"
					},
					"bindTo": "OPName"
				},
				"parentName": "ProfileContainer",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "OPExternalNumericId61f6a52b-0f9a-40de-a55b-b478afaeb94f",
				"values": {
					"layout": {
						"colSpan": 11,
						"rowSpan": 1,
						"column": 0,
						"row": 0,
						"layoutName": "Header"
					},
					"bindTo": "OPExternalNumericId"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "CreatedOn6347be89-954f-4f8a-9978-ea643a6681f4",
				"values": {
					"layout": {
						"colSpan": 11,
						"rowSpan": 1,
						"column": 13,
						"row": 0,
						"layoutName": "Header"
					},
					"bindTo": "CreatedOn"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "OPExternalId7547dba7-75d6-46b6-8a64-3a6b59238331",
				"values": {
					"layout": {
						"colSpan": 11,
						"rowSpan": 1,
						"column": 0,
						"row": 1,
						"layoutName": "Header"
					},
					"bindTo": "OPExternalId"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "OPNotesb4428f8e-53e0-48fc-b7bc-2ad13dfc3b5e",
				"values": {
					"layout": {
						"colSpan": 11,
						"rowSpan": 1,
						"column": 13,
						"row": 1,
						"layoutName": "Header"
					},
					"bindTo": "OPNotes"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 3
			},
			{
				"operation": "insert",
				"name": "ModifiedOn289d981d-df75-4cd9-ac3d-b4237a1c67f9",
				"values": {
					"layout": {
						"colSpan": 11,
						"rowSpan": 1,
						"column": 0,
						"row": 2,
						"layoutName": "Header"
					},
					"bindTo": "ModifiedOn"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 4
			},
			{
				"operation": "insert",
				"name": "ModifiedBy3522c0cb-b62c-43a7-87e3-8ea6b804596f",
				"values": {
					"layout": {
						"colSpan": 11,
						"rowSpan": 1,
						"column": 13,
						"row": 2,
						"layoutName": "Header"
					},
					"bindTo": "ModifiedBy"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 5
			},
			{
				"operation": "insert",
				"name": "OPExternalUpdatedAt0f82ad99-68fa-4092-9932-4c4a096a4a6e",
				"values": {
					"layout": {
						"colSpan": 11,
						"rowSpan": 1,
						"column": 0,
						"row": 3,
						"layoutName": "Header"
					},
					"bindTo": "OPExternalUpdatedAt"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 6
			},
			{
				"operation": "insert",
				"name": "CreatedByfac21f4f-c410-47b9-b176-02ea60a42c11",
				"values": {
					"layout": {
						"colSpan": 11,
						"rowSpan": 1,
						"column": 13,
						"row": 3,
						"layoutName": "Header"
					},
					"bindTo": "CreatedBy"
				},
				"parentName": "Header",
				"propertyName": "items",
				"index": 7
			},
			{
				"operation": "insert",
				"name": "GeneralInformation",
				"values": {
					"caption": {
						"bindTo": "Resources.Strings.GeneralInformationTabCaption"
					},
					"items": [],
					"order": 0
				},
				"parentName": "Tabs",
				"propertyName": "tabs",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "OPVehicleModelDetail",
				"values": {
					"itemType": 2,
					"markerValue": "added-detail"
				},
				"parentName": "GeneralInformation",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "NotesAndFilesTab",
				"values": {
					"caption": {
						"bindTo": "Resources.Strings.NotesAndFilesTabCaption"
					},
					"items": [],
					"order": 1
				},
				"parentName": "Tabs",
				"propertyName": "tabs",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "Files",
				"values": {
					"itemType": 2
				},
				"parentName": "NotesAndFilesTab",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "NotesControlGroup",
				"values": {
					"itemType": 15,
					"caption": {
						"bindTo": "Resources.Strings.NotesGroupCaption"
					},
					"items": []
				},
				"parentName": "NotesAndFilesTab",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "Notes",
				"values": {
					"bindTo": "OPNotes",
					"dataValueType": 1,
					"contentType": 4,
					"layout": {
						"column": 0,
						"row": 0,
						"colSpan": 24
					},
					"labelConfig": {
						"visible": false
					},
					"controlConfig": {
						"imageLoaded": {
							"bindTo": "insertImagesToNotes"
						},
						"images": {
							"bindTo": "NotesImagesCollection"
						}
					}
				},
				"parentName": "NotesControlGroup",
				"propertyName": "items",
				"index": 0
			}
		]/**SCHEMA_DIFF*/
	};
});

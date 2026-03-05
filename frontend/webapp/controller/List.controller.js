sap.ui.define([
  "sap/ui/core/mvc/Controller",
  "sap/ui/model/json/JSONModel",
  "sap/m/MessageBox"
], function (Controller, JSONModel, MessageBox) {
  "use strict";

  return Controller.extend("ui5.app.controller.List", {

    onInit: function () {

      const oModel = new JSONModel({
        data: [],
        busy: false,
        page: 1,
        pageSize: 10,
        totalPages: 1,
        search: ""
      });

      this.getView().setModel(oModel);

      this._debounce = null;
      this._abortController = null;

      this.getOwnerComponent()
        .getRouter()
        .getRoute("list")
        .attachPatternMatched(this._onRouteMatched, this);
    },

    onSync: async function () {

      const oModel = this.getView().getModel();

      oModel.setProperty("/busy", true);

      try {

        const response = await fetch("http://localhost:5245/sync", {
          method: "POST"
        });

        if (!response.ok) {
          throw new Error("Error synchronizing data.");
        }

        sap.m.MessageToast.show("Synchronization completed successfully.");

        await this._loadData();

      } catch (error) {

        sap.m.MessageBox.error(error.message);

      } finally {

        oModel.setProperty("/busy", false);
      }
    },

    _onRouteMatched: function () {
      this._loadData();
    },

    _loadData: async function () {

      const oModel = this.getView().getModel();
      const page = oModel.getProperty("/page");
      const pageSize = oModel.getProperty("/pageSize");
      const search = oModel.getProperty("/search");

      if (this._abortController) {
        this._abortController.abort();
      }

      this._abortController = new AbortController();

      oModel.setProperty("/busy", true);

      try {

        const response = await fetch(
          `http://localhost:5245/todos?page=${page}&pageSize=${pageSize}&title=${encodeURIComponent(search)}`,
          { signal: this._abortController.signal }
        );

        if (!response.ok) {
          throw new Error("Error loading tasks");
        }

        const data = await response.json();

        oModel.setProperty("/data", data.data || []);
        oModel.setProperty("/page", data.page);

        const totalPages = Math.ceil(data.totalItems / data.pageSize);
        oModel.setProperty("/totalPages", totalPages);

      } catch (error) {

        if (error.name !== "AbortError") {
          MessageBox.error(error.message);
        }

      } finally {
        oModel.setProperty("/busy", false);
      }
    },

    onToggleCompleted: async function (oEvent) {

      const oCheckBox = oEvent.getSource();
      const oContext = oCheckBox.getBindingContext();
      const id = oContext.getProperty("id");
      const newValue = oEvent.getParameter("selected");

      const oModel = this.getView().getModel();
      oModel.setProperty("/busy", true);

      try {

        const response = await fetch(`http://localhost:5245/todos/${id}`, {
          method: "PUT",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ completed: newValue })
        });

        if (!response.ok) {
          const text = await response.text();
          let errorMessage = "Updated error.";

          if (text) {
            try {
              const json = JSON.parse(text);
              errorMessage = json.message || errorMessage;
            } catch { }
          }

          throw new Error(errorMessage);
        }

        oContext.setProperty("completed", newValue);

      } catch (err) {

        oContext.setProperty("completed", !newValue);
        MessageBox.error(err.message);

      } finally {
        oModel.setProperty("/busy", false);
      }
    },

    onSearch: function (oEvent) {

      const value = oEvent.getParameter("newValue");
      const oModel = this.getView().getModel();

      clearTimeout(this._debounce);

      this._debounce = setTimeout(() => {
        oModel.setProperty("/search", value);
        oModel.setProperty("/page", 1);
        this._loadData();
      }, 500);
    },

    onNext: function () {

      const oModel = this.getView().getModel();
      const current = oModel.getProperty("/page");
      const totalPages = oModel.getProperty("/totalPages");

      if (current < totalPages) {
        oModel.setProperty("/page", current + 1);
        this._loadData();
      }
    },

    onPrevious: function () {

      const oModel = this.getView().getModel();
      const current = oModel.getProperty("/page");

      if (current > 1) {
        oModel.setProperty("/page", current - 1);
        this._loadData();
      }
    },

    onDetail: function (oEvent) {

      const oContext = oEvent.getSource().getBindingContext();
      const id = oContext.getProperty("id");

      this.getOwnerComponent()
        .getRouter()
        .navTo("detail", { id: id });
    }
  });
});

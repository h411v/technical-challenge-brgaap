sap.ui.define([
  "sap/ui/core/mvc/Controller",
  "sap/ui/model/json/JSONModel",
  "sap/m/MessageBox"
], function (Controller, JSONModel, MessageBox) {
  "use strict";

  return Controller.extend("ui5.app.controller.Detail", {

    onInit: function () {

      const oModel = new JSONModel({
        id: null,
        userId: null,
        title: "",
        completed: false,
        busy: false
      });

      this.getView().setModel(oModel);

      this._abortController = null;

      this.getOwnerComponent()
        .getRouter()
        .getRoute("detail")
        .attachPatternMatched(this._onRouteMatched, this);
    },

    _onRouteMatched: function (oEvent) {

      const id = oEvent.getParameter("arguments").id;

      if (!id) {
        MessageBox.error("Invalid id");
        return;
      }

      this._loadDetail(id);
    },

    _loadDetail: async function (id) {

      const oModel = this.getView().getModel();

      if (this._abortController) {
        this._abortController.abort();
      }

      this._abortController = new AbortController();

      oModel.setProperty("/busy", true);

      try {

        const response = await fetch(
          `http://localhost:5245/todos/${id}`,
          { signal: this._abortController.signal }
        );

        if (!response.ok) {
          throw new Error("Error loading details");
        }

        const data = await response.json();

        oModel.setData({
          ...data,
          busy: false
        });

      } catch (error) {

        if (error.name !== "AbortError") {
          MessageBox.error(error.message);
        }

      } finally {
        oModel.setProperty("/busy", false);
      }
    },

    onToggleCompleted: async function (oEvent) {

      const oModel = this.getView().getModel();
      const id = oModel.getProperty("/id");
      const newValue = oEvent.getParameter("selected");

      if (!id) {
        MessageBox.error("ID inválido");
        return;
      }

      oModel.setProperty("/busy", true);

      try {

        const response = await fetch(
          `http://localhost:5245/todos/${id}`,
          {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ completed: newValue })
          }
        );

        if (!response.ok) {
          throw new Error("Error update status");
        }

        oModel.setProperty("/completed", newValue);

      } catch (error) {

        oModel.setProperty("/completed", !newValue);
        MessageBox.error(error.message);

      } finally {
        oModel.setProperty("/busy", false);
      }
    },

    onBack: function () {

      const oRouter = this.getOwnerComponent().getRouter();
      const oHistory = sap.ui.core.routing.History.getInstance();
      const sPreviousHash = oHistory.getPreviousHash();

      if (sPreviousHash !== undefined) {
        window.history.go(-1);
      } else {
        oRouter.navTo("list", {}, true);
      }
    }

  });
});
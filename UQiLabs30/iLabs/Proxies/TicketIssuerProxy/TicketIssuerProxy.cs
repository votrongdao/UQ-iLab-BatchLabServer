﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3082
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by wsdl, Version=2.0.50727.42.
// 
namespace iLabs.Proxies.Ticketing {
    using System.Diagnostics;
    using System.Web.Services;
    using System.ComponentModel;
    using System.Web.Services.Protocols;
    using System;
    using System.Xml.Serialization;

    using iLabs.DataTypes.ProcessAgentTypes;
    using iLabs.DataTypes.SoapHeaderTypes;
    using iLabs.DataTypes.TicketingTypes;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="ITicketIssuer", Namespace="http://ilab.mit.edu/iLabs/Services")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AuthenticationHeader))]
    public partial class TicketIssuerProxy : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        public AgentAuthHeader AgentAuthHeaderValue;
        
        private System.Threading.SendOrPostCallback AddTicketOperationCompleted;
        
        private System.Threading.SendOrPostCallback CreateTicketOperationCompleted;
        
        private System.Threading.SendOrPostCallback RedeemTicketOperationCompleted;
        
        private System.Threading.SendOrPostCallback RequestTicketCancellationOperationCompleted;
        
        /// <remarks/>
        public TicketIssuerProxy() {
            this.Url = "http://localhost/ilab_WSDL/I_TicketIssuer.asmx";
        }
        
        /// <remarks/>
        public event AddTicketCompletedEventHandler AddTicketCompleted;
        
        /// <remarks/>
        public event CreateTicketCompletedEventHandler CreateTicketCompleted;
        
        /// <remarks/>
        public event RedeemTicketCompletedEventHandler RedeemTicketCompleted;
        
        /// <remarks/>
        public event RequestTicketCancellationCompletedEventHandler RequestTicketCancellationCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AgentAuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://ilab.mit.edu/iLabs/Services/AddTicket", RequestNamespace="http://ilab.mit.edu/iLabs/Services", ResponseNamespace="http://ilab.mit.edu/iLabs/Services", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool AddTicket([System.Xml.Serialization.XmlElementAttribute(Namespace="http://ilab.mit.edu/iLabs/type", IsNullable=true)] Coupon coupon, string type, string redeemerGuid, long duration, string payload) {
            object[] results = this.Invoke("AddTicket", new object[] {
                        coupon,
                        type,
                        redeemerGuid,
                        duration,
                        payload});
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginAddTicket(Coupon coupon, string type, string redeemerGuid, long duration, string payload, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("AddTicket", new object[] {
                        coupon,
                        type,
                        redeemerGuid,
                        duration,
                        payload}, callback, asyncState);
        }
        
        /// <remarks/>
        public bool EndAddTicket(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public void AddTicketAsync(Coupon coupon, string type, string redeemerGuid, long duration, string payload) {
            this.AddTicketAsync(coupon, type, redeemerGuid, duration, payload, null);
        }
        
        /// <remarks/>
        public void AddTicketAsync(Coupon coupon, string type, string redeemerGuid, long duration, string payload, object userState) {
            if ((this.AddTicketOperationCompleted == null)) {
                this.AddTicketOperationCompleted = new System.Threading.SendOrPostCallback(this.OnAddTicketOperationCompleted);
            }
            this.InvokeAsync("AddTicket", new object[] {
                        coupon,
                        type,
                        redeemerGuid,
                        duration,
                        payload}, this.AddTicketOperationCompleted, userState);
        }
        
        private void OnAddTicketOperationCompleted(object arg) {
            if ((this.AddTicketCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.AddTicketCompleted(this, new AddTicketCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AgentAuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://ilab.mit.edu/iLabs/Services/CreateTicket", RequestNamespace="http://ilab.mit.edu/iLabs/Services", ResponseNamespace="http://ilab.mit.edu/iLabs/Services", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(Namespace="http://ilab.mit.edu/iLabs/type", IsNullable=true)]
        public Coupon CreateTicket(string type, string redeemerGuid, long duration, string payload) {
            object[] results = this.Invoke("CreateTicket", new object[] {
                        type,
                        redeemerGuid,
                        duration,
                        payload});
            return ((Coupon)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginCreateTicket(string type, string redeemerGuid, long duration, string payload, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("CreateTicket", new object[] {
                        type,
                        redeemerGuid,
                        duration,
                        payload}, callback, asyncState);
        }
        
        /// <remarks/>
        public Coupon EndCreateTicket(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((Coupon)(results[0]));
        }
        
        /// <remarks/>
        public void CreateTicketAsync(string type, string redeemerGuid, long duration, string payload) {
            this.CreateTicketAsync(type, redeemerGuid, duration, payload, null);
        }
        
        /// <remarks/>
        public void CreateTicketAsync(string type, string redeemerGuid, long duration, string payload, object userState) {
            if ((this.CreateTicketOperationCompleted == null)) {
                this.CreateTicketOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCreateTicketOperationCompleted);
            }
            this.InvokeAsync("CreateTicket", new object[] {
                        type,
                        redeemerGuid,
                        duration,
                        payload}, this.CreateTicketOperationCompleted, userState);
        }
        
        private void OnCreateTicketOperationCompleted(object arg) {
            if ((this.CreateTicketCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.CreateTicketCompleted(this, new CreateTicketCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AgentAuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://ilab.mit.edu/iLabs/Services/RedeemTicket", RequestNamespace="http://ilab.mit.edu/iLabs/Services", ResponseNamespace="http://ilab.mit.edu/iLabs/Services", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(Namespace="http://ilab.mit.edu/iLabs/type", IsNullable=true)]
        public Ticket RedeemTicket([System.Xml.Serialization.XmlElementAttribute(Namespace="http://ilab.mit.edu/iLabs/type", IsNullable=true)] Coupon coupon, string type, string redeemerGuid) {
            object[] results = this.Invoke("RedeemTicket", new object[] {
                        coupon,
                        type,
                        redeemerGuid});
            return ((Ticket)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginRedeemTicket(Coupon coupon, string type, string redeemerGuid, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("RedeemTicket", new object[] {
                        coupon,
                        type,
                        redeemerGuid}, callback, asyncState);
        }
        
        /// <remarks/>
        public Ticket EndRedeemTicket(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((Ticket)(results[0]));
        }
        
        /// <remarks/>
        public void RedeemTicketAsync(Coupon coupon, string type, string redeemerGuid) {
            this.RedeemTicketAsync(coupon, type, redeemerGuid, null);
        }
        
        /// <remarks/>
        public void RedeemTicketAsync(Coupon coupon, string type, string redeemerGuid, object userState) {
            if ((this.RedeemTicketOperationCompleted == null)) {
                this.RedeemTicketOperationCompleted = new System.Threading.SendOrPostCallback(this.OnRedeemTicketOperationCompleted);
            }
            this.InvokeAsync("RedeemTicket", new object[] {
                        coupon,
                        type,
                        redeemerGuid}, this.RedeemTicketOperationCompleted, userState);
        }
        
        private void OnRedeemTicketOperationCompleted(object arg) {
            if ((this.RedeemTicketCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.RedeemTicketCompleted(this, new RedeemTicketCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AgentAuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://ilab.mit.edu/iLabs/Services/RequestTicketCancellation", RequestNamespace="http://ilab.mit.edu/iLabs/Services", ResponseNamespace="http://ilab.mit.edu/iLabs/Services", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool RequestTicketCancellation([System.Xml.Serialization.XmlElementAttribute(Namespace="http://ilab.mit.edu/iLabs/type", IsNullable=true)] Coupon coupon, string type, string redeemerGuid) {
            object[] results = this.Invoke("RequestTicketCancellation", new object[] {
                        coupon,
                        type,
                        redeemerGuid});
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginRequestTicketCancellation(Coupon coupon, string type, string redeemerGuid, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("RequestTicketCancellation", new object[] {
                        coupon,
                        type,
                        redeemerGuid}, callback, asyncState);
        }
        
        /// <remarks/>
        public bool EndRequestTicketCancellation(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public void RequestTicketCancellationAsync(Coupon coupon, string type, string redeemerGuid) {
            this.RequestTicketCancellationAsync(coupon, type, redeemerGuid, null);
        }
        
        /// <remarks/>
        public void RequestTicketCancellationAsync(Coupon coupon, string type, string redeemerGuid, object userState) {
            if ((this.RequestTicketCancellationOperationCompleted == null)) {
                this.RequestTicketCancellationOperationCompleted = new System.Threading.SendOrPostCallback(this.OnRequestTicketCancellationOperationCompleted);
            }
            this.InvokeAsync("RequestTicketCancellation", new object[] {
                        coupon,
                        type,
                        redeemerGuid}, this.RequestTicketCancellationOperationCompleted, userState);
        }
        
        private void OnRequestTicketCancellationOperationCompleted(object arg) {
            if ((this.RequestTicketCancellationCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.RequestTicketCancellationCompleted(this, new RequestTicketCancellationCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
    }
    
   
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    public delegate void AddTicketCompletedEventHandler(object sender, AddTicketCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class AddTicketCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal AddTicketCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public bool Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((bool)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    public delegate void CreateTicketCompletedEventHandler(object sender, CreateTicketCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class CreateTicketCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal CreateTicketCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public Coupon Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((Coupon)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    public delegate void RedeemTicketCompletedEventHandler(object sender, RedeemTicketCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class RedeemTicketCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal RedeemTicketCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public Ticket Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((Ticket)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    public delegate void RequestTicketCancellationCompletedEventHandler(object sender, RequestTicketCancellationCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.42")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class RequestTicketCancellationCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal RequestTicketCancellationCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public bool Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((bool)(this.results[0]));
            }
        }
    }
}
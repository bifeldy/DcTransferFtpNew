﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.42000
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict Off
Option Explicit On

Imports System
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Xml.Serialization

'
'This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.42000.
'
Namespace DSI_WS
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0"),  _
     System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.ComponentModel.DesignerCategoryAttribute("code"),  _
     System.Web.Services.WebServiceBindingAttribute(Name:="DSI_WSSoap", [Namespace]:="http://tempuri.org/")>  _
    Partial Public Class DSI_WS
        Inherits System.Web.Services.Protocols.SoapHttpClientProtocol
        
        Private Get_DSISumOperationCompleted As System.Threading.SendOrPostCallback
        
        Private Get_DSIDetailOperationCompleted As System.Threading.SendOrPostCallback
        
        Private useDefaultCredentialsSetExplicitly As Boolean
        
        '''<remarks/>
        Public Sub New()
            MyBase.New
            Me.Url = Global.DCTransferFTP.My.MySettings.Default.DCTransferFTP_DSI_WS_DSI_WS
            If (Me.IsLocalFileSystemWebService(Me.Url) = true) Then
                Me.UseDefaultCredentials = true
                Me.useDefaultCredentialsSetExplicitly = false
            Else
                Me.useDefaultCredentialsSetExplicitly = true
            End If
        End Sub
        
        Public Shadows Property Url() As String
            Get
                Return MyBase.Url
            End Get
            Set
                If (((Me.IsLocalFileSystemWebService(MyBase.Url) = true)  _
                            AndAlso (Me.useDefaultCredentialsSetExplicitly = false))  _
                            AndAlso (Me.IsLocalFileSystemWebService(value) = false)) Then
                    MyBase.UseDefaultCredentials = false
                End If
                MyBase.Url = value
            End Set
        End Property
        
        Public Shadows Property UseDefaultCredentials() As Boolean
            Get
                Return MyBase.UseDefaultCredentials
            End Get
            Set
                MyBase.UseDefaultCredentials = value
                Me.useDefaultCredentialsSetExplicitly = true
            End Set
        End Property
        
        '''<remarks/>
        Public Event Get_DSISumCompleted As Get_DSISumCompletedEventHandler
        
        '''<remarks/>
        Public Event Get_DSIDetailCompleted As Get_DSIDetailCompletedEventHandler
        
        '''<remarks/>
        <System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/Get_DSISum", RequestNamespace:="http://tempuri.org/", ResponseNamespace:="http://tempuri.org/", Use:=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle:=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)>  _
        Public Function Get_DSISum(ByVal periode As Date) As String
            Dim results() As Object = Me.Invoke("Get_DSISum", New Object() {periode})
            Return CType(results(0),String)
        End Function
        
        '''<remarks/>
        Public Overloads Sub Get_DSISumAsync(ByVal periode As Date)
            Me.Get_DSISumAsync(periode, Nothing)
        End Sub
        
        '''<remarks/>
        Public Overloads Sub Get_DSISumAsync(ByVal periode As Date, ByVal userState As Object)
            If (Me.Get_DSISumOperationCompleted Is Nothing) Then
                Me.Get_DSISumOperationCompleted = AddressOf Me.OnGet_DSISumOperationCompleted
            End If
            Me.InvokeAsync("Get_DSISum", New Object() {periode}, Me.Get_DSISumOperationCompleted, userState)
        End Sub
        
        Private Sub OnGet_DSISumOperationCompleted(ByVal arg As Object)
            If (Not (Me.Get_DSISumCompletedEvent) Is Nothing) Then
                Dim invokeArgs As System.Web.Services.Protocols.InvokeCompletedEventArgs = CType(arg,System.Web.Services.Protocols.InvokeCompletedEventArgs)
                RaiseEvent Get_DSISumCompleted(Me, New Get_DSISumCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState))
            End If
        End Sub
        
        '''<remarks/>
        <System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/Get_DSIDetail", RequestNamespace:="http://tempuri.org/", ResponseNamespace:="http://tempuri.org/", Use:=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle:=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)>  _
        Public Function Get_DSIDetail(ByVal periode As Date) As String
            Dim results() As Object = Me.Invoke("Get_DSIDetail", New Object() {periode})
            Return CType(results(0),String)
        End Function
        
        '''<remarks/>
        Public Overloads Sub Get_DSIDetailAsync(ByVal periode As Date)
            Me.Get_DSIDetailAsync(periode, Nothing)
        End Sub
        
        '''<remarks/>
        Public Overloads Sub Get_DSIDetailAsync(ByVal periode As Date, ByVal userState As Object)
            If (Me.Get_DSIDetailOperationCompleted Is Nothing) Then
                Me.Get_DSIDetailOperationCompleted = AddressOf Me.OnGet_DSIDetailOperationCompleted
            End If
            Me.InvokeAsync("Get_DSIDetail", New Object() {periode}, Me.Get_DSIDetailOperationCompleted, userState)
        End Sub
        
        Private Sub OnGet_DSIDetailOperationCompleted(ByVal arg As Object)
            If (Not (Me.Get_DSIDetailCompletedEvent) Is Nothing) Then
                Dim invokeArgs As System.Web.Services.Protocols.InvokeCompletedEventArgs = CType(arg,System.Web.Services.Protocols.InvokeCompletedEventArgs)
                RaiseEvent Get_DSIDetailCompleted(Me, New Get_DSIDetailCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState))
            End If
        End Sub
        
        '''<remarks/>
        Public Shadows Sub CancelAsync(ByVal userState As Object)
            MyBase.CancelAsync(userState)
        End Sub
        
        Private Function IsLocalFileSystemWebService(ByVal url As String) As Boolean
            If ((url Is Nothing)  _
                        OrElse (url Is String.Empty)) Then
                Return false
            End If
            Dim wsUri As System.Uri = New System.Uri(url)
            If ((wsUri.Port >= 1024)  _
                        AndAlso (String.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) = 0)) Then
                Return true
            End If
            Return false
        End Function
    End Class
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")>  _
    Public Delegate Sub Get_DSISumCompletedEventHandler(ByVal sender As Object, ByVal e As Get_DSISumCompletedEventArgs)
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0"),  _
     System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.ComponentModel.DesignerCategoryAttribute("code")>  _
    Partial Public Class Get_DSISumCompletedEventArgs
        Inherits System.ComponentModel.AsyncCompletedEventArgs
        
        Private results() As Object
        
        Friend Sub New(ByVal results() As Object, ByVal exception As System.Exception, ByVal cancelled As Boolean, ByVal userState As Object)
            MyBase.New(exception, cancelled, userState)
            Me.results = results
        End Sub
        
        '''<remarks/>
        Public ReadOnly Property Result() As String
            Get
                Me.RaiseExceptionIfNecessary
                Return CType(Me.results(0),String)
            End Get
        End Property
    End Class
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")>  _
    Public Delegate Sub Get_DSIDetailCompletedEventHandler(ByVal sender As Object, ByVal e As Get_DSIDetailCompletedEventArgs)
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0"),  _
     System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.ComponentModel.DesignerCategoryAttribute("code")>  _
    Partial Public Class Get_DSIDetailCompletedEventArgs
        Inherits System.ComponentModel.AsyncCompletedEventArgs
        
        Private results() As Object
        
        Friend Sub New(ByVal results() As Object, ByVal exception As System.Exception, ByVal cancelled As Boolean, ByVal userState As Object)
            MyBase.New(exception, cancelled, userState)
            Me.results = results
        End Sub
        
        '''<remarks/>
        Public ReadOnly Property Result() As String
            Get
                Me.RaiseExceptionIfNecessary
                Return CType(Me.results(0),String)
            End Get
        End Property
    End Class
End Namespace

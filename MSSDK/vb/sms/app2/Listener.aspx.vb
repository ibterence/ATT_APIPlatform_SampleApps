﻿' <copyright file="Listener.aspx.vb" company="AT&amp;T">
' Licensed by AT&amp;T under 'Software Development Kit Tools Agreement.' 2013
' TERMS AND CONDITIONS FOR USE, REPRODUCTION, AND DISTRIBUTION: http://developer.att.com
' Copyright 2013 AT&amp;T Intellectual Property. All rights reserved. http://developer.att.com
' For more information contact developer.support@att.com
' </copyright>

#Region "References"
Imports System.Configuration
Imports System.IO
Imports System.Text
Imports System.Web
Imports System.Web.Script.Serialization
Imports ATT_MSSDK
Imports ATT_MSSDK.SMSv3
#End Region

''' <summary>
''' Listener class for saving message counts.
''' </summary>
Partial Public Class Listener
    Inherits System.Web.UI.Page

#Region "Listener Application Events"

    ''' <summary>
    ''' This method called when the page is loaded into the browser. This method requests input stream and parses it to get message counts.
    ''' </summary>
    ''' <param name="sender">object, which invoked this method</param>
    ''' <param name="e">EventArgs, which specifies arguments specific to this method</param>
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim stream As System.IO.Stream = Request.InputStream

        If stream IsNot Nothing Then
            Dim message As ReceivedSMS = RequestFactory.GetSMS(stream)
            If message IsNot Nothing Then
                Me.SaveMessageCount(message)
                Me.SaveMessage(message)
            End If
        End If
    End Sub

#End Region

#Region "Listener Application specific functions"

    ''' <summary>
    ''' This method reads the incoming message and decides on to which message count needs to be updated.
    ''' This method invokes another method to write the count to file
    ''' </summary>
    ''' <param name="message">ReceivedSMS, message received from Request</param>
    Private Sub SaveMessageCount(message As ReceivedSMS)
        If Not String.IsNullOrEmpty(message.Message) Then
            Dim messageText As String = message.Message.Trim().ToLower()

            Dim filePathConfigKey As String = String.Empty
            Select Case messageText
                Case "basketball"
                    filePathConfigKey = "BasketBallFilePath"
                    Exit Select
                Case "football"
                    filePathConfigKey = "FootBallFilePath"
                    Exit Select
                Case "baseball"
                    filePathConfigKey = "BaseBallFilePath"
                    Exit Select
            End Select

            If Not String.IsNullOrEmpty(filePathConfigKey) Then
                Me.WriteToFile(filePathConfigKey)
            End If
        End If
    End Sub

    ''' <summary>
    ''' This method gets the file name, reads from the file, increments the count(if any) and writes back to the file.
    ''' </summary>
    ''' <param name="filePathConfigKey">string, parameter which specifies the config key to the file</param>
    Private Sub WriteToFile(filePathConfigKey As String)
        Dim filePath As String = ConfigurationManager.AppSettings(filePathConfigKey)

        Dim count As Integer = 0
        Using streamReader As StreamReader = File.OpenText(Request.MapPath(filePath))
            count = Convert.ToInt32(streamReader.ReadToEnd())
            streamReader.Close()
        End Using

        count = count + 1

        Using streamWriter As StreamWriter = File.CreateText(Request.MapPath(filePath))
            streamWriter.Write(count)
            streamWriter.Close()
        End Using
    End Sub

    ''' <summary>
    ''' This method reads the incoming message and stores the received message details.
    ''' </summary>
    ''' <param name="message">ReceivedSMS, message received from Request</param>
    Private Sub SaveMessage(message As ReceivedSMS)
        Dim filePath As String = ConfigurationManager.AppSettings("MessagesFilePath")

        Dim messageLineToStore As String = message.DateTime.ToString() & "_-_-" & message.MessageId.ToString() & "_-_-" & message.Message.ToString() & "_-_-" & message.SenderAddress.ToString() & "_-_-" & message.DestinationAddress.ToString()

        Using streamWriter As StreamWriter = File.AppendText(Request.MapPath(filePath))
            streamWriter.WriteLine(messageLineToStore)
            streamWriter.Close()
        End Using
    End Sub

#End Region

End Class
﻿Imports System.IO
Imports System.Text

Public Class SyncSetForm
    Dim cn_chk As Integer = 0 'Номер столбца с чекбоксом
    Dim cn_nm As Integer = 1 'Номер столбца с именем
    Dim collor_cat As System.Drawing.Color = System.Drawing.Color.LavenderBlush
    Dim collor_file As System.Drawing.Color = System.Drawing.Color.PaleTurquoise

    Private Sub FilesForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim chk As New DataGridViewCheckBoxColumn()
        Dim dstr(3) As String
        With DataGridView2
            .Columns.Add(chk)
            .Columns.Add("obj", "Объект")

            .Columns.Item(cn_chk).Name = "All Files"
            .Columns(cn_nm).Width = 570
            .Columns(cn_chk).Width = 50
        End With

        With OpenFileDialog1
            .Multiselect = True
            .Title = "Окно выбора файлов"
        End With

        On Error GoTo err1
        addToCoBox() 'Читаем список фильтров
        addToGrid() 'добавление элементов в таблицу

        Exit Sub
err1:
        If MainForm.bool_configFileExist = False Then
            Dim m As MsgBoxResult
            m = MsgBox("Конфигурационный файл не найден. Создать новый?", vbOKCancel, vbQuestion)
            If m = 1 Then
                createConfigFile()
                MainForm.initConfig()
            End If
            Resume
        End If
        MsgBox("Ошибка номер " & Err.Number & ". " & Err.Description, vbCritical, "Ошибка")
    End Sub
    Sub createConfigFile()
        Dim xd As XDocument =
                    <?xml version="1.0" encoding="utf-8"?>
                    <Root>
                        <Settings>
                            <IP>127.0.0.1</IP>
                            <prjDirSet>Manual</prjDirSet>
                            <prjDir>C:\Dynamics\</prjDir>
                            <bkpDir>C:\Users\user\Desktop\backupTest</bkpDir>
                        </Settings>
                        <Backups>
                            <Backup Enable="True">C:\Dynamics\SSNTestBackup</Backup>
                        </Backups>
                        <Sync></Sync>
                        <Filters></Filters>
                    </Root>
        MainForm.xdoc = xd
        MainForm.xdoc.Save(MainForm.SyncFileName)
    End Sub
    Public Sub addToCoBox()

        Dim filterStr As String
        Dim sh As Integer = 0

        ComboBox1.Items.Clear()
        ComboBox1.Items.Add("") 'добавляем пустой фильтр

        For Each xe As XElement In MainForm.xdoc.Element("Root").Element("Filters").Elements("Filter") 'Читаем список фильтров и добавляем его в комбобокс
            ComboBox1.Items.Add(xe.Value)
        Next
        ComboBox1.SelectedIndex = 0

        For Each item As Object In ComboBox1.Items
            sh = sh + 1
            If item = "" Then filterStr = Trim(filterStr) & "AllFiles (*.*)|*.*"
            If item <> "" Then filterStr = Trim(filterStr) & "Filter (" & item & ")|" & item
            If sh = ComboBox1.Items.Count Then Exit For
            filterStr = filterStr & "|"
        Next

        OpenFileDialog1.Filter = Trim(filterStr)

    End Sub

    Public Function xmlLoad() As Boolean 'загружаем хмл со списком файлов. И сохраняем его в коллекцию  При удачной загрузке возвращает true
        On Error GoTo err1
        Dim i As Integer = 0
        MainForm.fileCollect.Clear()
        MainForm.catCollect.Clear()

        If MainForm.xdoc.Element("Root").Elements("Sync") Is Nothing Then
            xmlLoad = False
            Exit Function
        End If

        For Each xe As XElement In MainForm.xdoc.Element("Root").Elements("Sync").Elements("File")
            MainForm.fileCollect.Add(New FileObj)
            MainForm.fileCollect.Item(i).Name = MainForm.getfName(xe.Value, 0)
            MainForm.fileCollect.Item(i).Location = MainForm.getfName(xe.Value, 1)
            i = i + 1
        Next

        For Each xer As XElement In MainForm.xdoc.Element("Root").Element("Sync").Elements("Catalog")
            MainForm.catCollect.Add(xer.Value)
        Next

        xmlLoad = True

        Exit Function
err1:
        MsgBox("Ошибка номер " & Err.Number & ". " & Err.Description, vbCritical, "Ошибка")
        xmlLoad = False
    End Function

    Sub addToGrid() 'добавляем в табличку
        Dim k As Integer = 0
        DataGridView2.Rows.Clear()
        For Each xer As XElement In MainForm.xdoc.Element("Root").Elements("Sync").Elements("Catalog") ' Добавляем каталоги
            DataGridView2.Rows.Add()
            DataGridView2.Rows.Item(k).Cells(cn_nm).Value = xer.Value
            DataGridView2.Rows.Item(k).Cells(cn_chk).Value = xer.Attribute("allFiles").Value
            DataGridView2.Rows.Item(k).Cells(cn_nm).Style.BackColor = collor_cat ' красим в нужный цвет
            k = k + 1
        Next
        For Each xer As XElement In MainForm.xdoc.Element("Root").Element("Sync").Elements("File") 'Файлы
            DataGridView2.Rows.Add()
            DataGridView2.Rows.Item(k).Cells(cn_nm).Value = xer.Value
            DataGridView2.Rows.Item(k).Cells(cn_nm).Style.BackColor = collor_file ' красим в нужный цвет
            DataGridView2.Rows.Item(k).Cells(cn_chk).Style.BackColor = SystemColors.Control ' красим в нужный цвет
            DataGridView2.Rows.Item(k).Cells(cn_chk).ReadOnly = True
            k = k + 1
        Next
    End Sub

    Function getfName(ByVal s As String, Optional b As Integer = 0) As String
        Dim q()
        q = Split(s, "\")

        If b = 0 Then
            getfName = q(q.Length - 1)
        Else
            For i = 0 To q.Length - 2
                getfName = getfName & q(i) & "\"
            Next
        End If

        getfName = Trim(getfName)

    End Function

    Private Sub Button1_Click(sender As Object, e As EventArgs)
        If Not xmlLoad() Then Me.Close()
    End Sub

    Private Sub But_add_cat_Click(sender As Object, e As EventArgs) Handles But_add_cat.Click
        Dim dr As DialogResult
        dr = FolderBrowserDialog1.ShowDialog()
        If FolderBrowserDialog1.SelectedPath <> "" And dr <> DialogResult.Cancel Then
            DataGridView2.Rows.Add()
            DataGridView2.Rows.Item(DataGridView2.Rows.Count - 1).Cells(cn_nm).Value = FolderBrowserDialog1.SelectedPath
            DataGridView2.Rows.Item(DataGridView2.Rows.Count - 1).Cells(cn_chk).Value = False
            DataGridView2.Rows.Item(DataGridView2.Rows.Count - 1).Cells(cn_nm).Style.BackColor = collor_cat ' красим в нужный цвет
        End If

    End Sub

    Private Sub But_add_file_Click(sender As Object, e As EventArgs) Handles But_add_file.Click
        Dim dr As DialogResult
        OpenFileDialog1.ShowDialog()
    End Sub


    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

        DataGridView2.Rows.Clear()


    End Sub


    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        OpenFileDialog1.FilterIndex = ComboBox1.SelectedIndex + 1
    End Sub

    Private Sub but_addFilter_Click(sender As Object, e As EventArgs) Handles but_addFilter.Click
        FiltersForm.Show()
    End Sub

    Private Sub But_save_Click(sender As Object, e As EventArgs) Handles But_save.Click ' сохраняем и еще раз загружаем в коллекцию список файлов

        If Not MainForm.bool_configFileExist Then
            Dim fs As FileStream = File.Create(MainForm.SyncFileName)
            Dim info As Byte() = New UTF8Encoding(True).GetBytes("<?xml version=""1.0"" encoding=""utf-8""?>" & Chr(13) & "<Root>" & Chr(13) & "</Root>")
            fs.Write(info, 0, info.Length)
            fs.Close()
            'MainForm.xdoc = XDocument.Load(MainForm.SyncFileName)
        End If

        If Not MainForm.xdoc.Element("Root").Element("Sync") Is Nothing Then MainForm.xdoc.Element("Root").Element("Sync").Remove() 'удаляем существующий список объектов

        Dim xmlTree1 As XElement =
            <Sync>
            </Sync>

        For Each dataElem As DataGridViewRow In DataGridView2.Rows
            If dataElem.Cells(cn_nm).Value <> "" Then
                If dataElem.Cells(cn_nm).Style.BackColor = collor_cat Then
                    xmlTree1.Add(New XElement(<Catalog allFiles=<%= dataElem.Cells(cn_chk).Value %>><%= dataElem.Cells(cn_nm).Value %></Catalog>))
                ElseIf dataElem.Cells(cn_nm).Style.BackColor = collor_file Then
                    xmlTree1.Add(New XElement(<File><%= dataElem.Cells(cn_nm).Value %></File>))
                End If
            End If
        Next

        MainForm.xdoc.Element("Root").Add(New XElement(xmlTree1))
        '__________________________________________________________________



        ''______________________________________________________________сохраняем список каталогов
        'If Not MainForm.xdoc.Element("Root").Element("Catalogs") Is Nothing Then MainForm.xdoc.Element("Root").Element("Catalogs").Remove()
        'Dim xmlTree2 As XElement = _
        '    <Catalogs>
        '    </Catalogs>

        'For Each dataElem As DataGridViewRow In DataGridView2.Rows
        '    If dataElem.Cells(1).Value <> "True" Then dataElem.Cells(1).Value = "False"
        '    If dataElem.Cells(0).Value <> "" Then
        '        xmlTree2.Add(New XElement(<Catalog allFiles=<%= dataElem.Cells(1).Value %>><%= dataElem.Cells(0).Value %></Catalog>))
        '    End If
        'Next

        ''__________________________________________________________________

        'MainForm.xdoc.Element("Root").Add(New XElement(xmlTree2))

        'If RadioButton1.Checked = True Then
        '    MainForm.xElem_SynType.Value = "Files"
        'Else
        '    MainForm.xElem_SynType.Value = "Catalogs"
        'End If


        MainForm.xdoc.Save(MainForm.SyncFileName)
        xmlLoad()

        MsgBox("Успешно сохранено!", vbOKOnly)

    End Sub

    Private Sub OpenFileDialog1_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles OpenFileDialog1.FileOk
        Dim rowsCount = DataGridView2.Rows.Count - 1
        If rowsCount = -1 Then
            DataGridView2.Rows.Add()
            rowsCount = 0
        End If
        rowsCount = rowsCount + 1
        For i = 0 To OpenFileDialog1.FileNames().Length - 1
            DataGridView2.Rows.Add()
            DataGridView2.Rows.Item(rowsCount).Cells(cn_nm).Value = OpenFileDialog1.FileNames(i) 'getfName(OpenFileDialog1.FileNames(i))
            DataGridView2.Rows.Item(rowsCount).Cells(cn_chk).Style.BackColor = SystemColors.Control ' красим в нужный цвет
            DataGridView2.Rows.Item(rowsCount).Cells(cn_chk).ReadOnly = True

            DataGridView2.Rows.Item(rowsCount).Cells(cn_nm).Style.BackColor = collor_file ' красим в нужный цвет
            rowsCount = rowsCount + 1
        Next

    End Sub


    'Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles Button1.Click
    '    For i = 0 To DataGridView1.SelectedRows.Count - 1
    '        DataGridView1.SelectedRows(i).Cells.RemoveAt(i)
    '        DataGridView1.Rows.RemoveAt()
    '    Next

    'End Sub

End Class
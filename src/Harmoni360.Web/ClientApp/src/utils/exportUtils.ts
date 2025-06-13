import { saveAs } from 'file-saver';
import * as XLSX from 'xlsx';
import jsPDF from 'jspdf';
import 'jspdf-autotable';
import { InspectionDto, InspectionDashboardDto } from '../types/inspection';
import { format } from 'date-fns';

declare module 'jspdf' {
  interface jsPDF {
    autoTable: (options: any) => jsPDF;
  }
}

export interface ExportOptions {
  filename?: string;
  title?: string;
  includeCharts?: boolean;
}

export const exportInspectionsToExcel = (
  inspections: InspectionDto[],
  options: ExportOptions = {}
) => {
  const { filename = 'inspections-export' } = options;

  // Prepare data for Excel
  const excelData = inspections.map(inspection => ({
    'Inspection Number': inspection.inspectionNumber,
    'Title': inspection.title,
    'Type': inspection.typeName,
    'Category': inspection.categoryName,
    'Status': inspection.statusName,
    'Priority': inspection.priorityName,
    'Inspector': inspection.inspectorName,
    'Department': inspection.departmentName,
    'Risk Level': inspection.riskLevelName,
    'Scheduled Date': format(new Date(inspection.scheduledDate), 'yyyy-MM-dd HH:mm'),
    'Started Date': inspection.startedDate ? format(new Date(inspection.startedDate), 'yyyy-MM-dd HH:mm') : '',
    'Completed Date': inspection.completedDate ? format(new Date(inspection.completedDate), 'yyyy-MM-dd HH:mm') : '',
    'Items Count': inspection.itemsCount,
    'Completed Items': inspection.completedItemsCount,
    'Findings Count': inspection.findingsCount,
    'Critical Findings': inspection.criticalFindingsCount,
    'Estimated Duration (min)': inspection.estimatedDurationMinutes,
    'Actual Duration (min)': inspection.actualDurationMinutes || '',
    'Is Overdue': inspection.isOverdue ? 'Yes' : 'No',
    'Created Date': format(new Date(inspection.createdAt), 'yyyy-MM-dd HH:mm'),
    'Created By': inspection.createdBy
  }));

  // Create workbook
  const ws = XLSX.utils.json_to_sheet(excelData);
  const wb = XLSX.utils.book_new();
  
  // Set column widths
  const colWidths = [
    { wch: 15 }, // Inspection Number
    { wch: 30 }, // Title
    { wch: 12 }, // Type
    { wch: 12 }, // Category
    { wch: 12 }, // Status
    { wch: 10 }, // Priority
    { wch: 20 }, // Inspector
    { wch: 15 }, // Department
    { wch: 12 }, // Risk Level
    { wch: 16 }, // Scheduled Date
    { wch: 16 }, // Started Date
    { wch: 16 }, // Completed Date
    { wch: 10 }, // Items Count
    { wch: 12 }, // Completed Items
    { wch: 12 }, // Findings Count
    { wch: 14 }, // Critical Findings
    { wch: 16 }, // Estimated Duration
    { wch: 14 }, // Actual Duration
    { wch: 10 }, // Is Overdue
    { wch: 16 }, // Created Date
    { wch: 15 }  // Created By
  ];
  ws['!cols'] = colWidths;

  // Add worksheet to workbook
  XLSX.utils.book_append_sheet(wb, ws, 'Inspections');

  // Generate Excel file
  const excelBuffer = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });
  const data = new Blob([excelBuffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
  
  saveAs(data, `${filename}.xlsx`);
};

export const exportInspectionsToPDF = (
  inspections: InspectionDto[],
  options: ExportOptions = {}
) => {
  const { filename = 'inspections-report', title = 'Inspection Report' } = options;

  const doc = new jsPDF('l', 'mm', 'a4'); // Landscape orientation
  const pageWidth = doc.internal.pageSize.width;
  
  // Add title
  doc.setFontSize(16);
  doc.setFont('helvetica', 'bold');
  doc.text(title, pageWidth / 2, 15, { align: 'center' });
  
  // Add generation date
  doc.setFontSize(10);
  doc.setFont('helvetica', 'normal');
  doc.text(`Generated on: ${format(new Date(), 'yyyy-MM-dd HH:mm')}`, pageWidth / 2, 22, { align: 'center' });

  // Prepare table data
  const tableColumns = [
    'Number',
    'Title',
    'Type',
    'Status',
    'Priority',
    'Inspector',
    'Scheduled',
    'Items',
    'Findings'
  ];

  const tableRows = inspections.map(inspection => [
    inspection.inspectionNumber,
    inspection.title.length > 25 ? inspection.title.substring(0, 22) + '...' : inspection.title,
    inspection.typeName,
    inspection.statusName,
    inspection.priorityName,
    inspection.inspectorName,
    format(new Date(inspection.scheduledDate), 'MM/dd/yy'),
    `${inspection.completedItemsCount}/${inspection.itemsCount}`,
    inspection.findingsCount.toString()
  ]);

  // Add table
  doc.autoTable({
    head: [tableColumns],
    body: tableRows,
    startY: 30,
    styles: {
      fontSize: 8,
      cellPadding: 2,
    },
    headStyles: {
      fillColor: [41, 128, 185],
      textColor: 255,
      fontStyle: 'bold'
    },
    alternateRowStyles: {
      fillColor: [240, 240, 240]
    },
    columnStyles: {
      0: { cellWidth: 20 }, // Number
      1: { cellWidth: 40 }, // Title
      2: { cellWidth: 20 }, // Type
      3: { cellWidth: 20 }, // Status
      4: { cellWidth: 18 }, // Priority
      5: { cellWidth: 30 }, // Inspector
      6: { cellWidth: 20 }, // Scheduled
      7: { cellWidth: 15 }, // Items
      8: { cellWidth: 15 }  // Findings
    }
  });

  // Add footer with page numbers
  const pageCount = (doc as any).internal.getNumberOfPages();
  for (let i = 1; i <= pageCount; i++) {
    doc.setPage(i);
    doc.setFontSize(8);
    doc.text(
      `Page ${i} of ${pageCount}`,
      pageWidth - 20,
      doc.internal.pageSize.height - 10,
      { align: 'right' }
    );
  }

  // Save the PDF
  doc.save(`${filename}.pdf`);
};

export const exportDashboardToPDF = (
  dashboardData: InspectionDashboardDto,
  options: ExportOptions = {}
) => {
  const { filename = 'inspection-dashboard-report', title = 'Inspection Dashboard Report' } = options;

  const doc = new jsPDF('p', 'mm', 'a4'); // Portrait orientation
  const pageWidth = doc.internal.pageSize.width;
  let yPosition = 20;

  // Add title
  doc.setFontSize(18);
  doc.setFont('helvetica', 'bold');
  doc.text(title, pageWidth / 2, yPosition, { align: 'center' });
  yPosition += 10;

  // Add generation date
  doc.setFontSize(12);
  doc.setFont('helvetica', 'normal');
  doc.text(`Generated on: ${format(new Date(), 'yyyy-MM-dd HH:mm')}`, pageWidth / 2, yPosition, { align: 'center' });
  yPosition += 20;

  // Add KPI summary
  doc.setFontSize(14);
  doc.setFont('helvetica', 'bold');
  doc.text('Key Performance Indicators', 20, yPosition);
  yPosition += 10;

  const kpiData = [
    ['Total Inspections', dashboardData.totalInspections.toString()],
    ['In Progress', dashboardData.inProgressInspections.toString()],
    ['Completed', dashboardData.completedInspections.toString()],
    ['Overdue', dashboardData.overdueInspections.toString()],
    ['Critical Findings', dashboardData.criticalFindings.toString()],
    ['Compliance Rate', `${dashboardData.complianceRate}%`],
    ['Avg. Completion Time', `${dashboardData.averageCompletionTime}h`]
  ];

  doc.autoTable({
    body: kpiData,
    startY: yPosition,
    styles: {
      fontSize: 11,
      cellPadding: 3,
    },
    columnStyles: {
      0: { cellWidth: 60, fontStyle: 'bold' },
      1: { cellWidth: 40, halign: 'right' }
    }
  });

  yPosition = (doc as any).lastAutoTable.finalY + 15;

  // Add status distribution
  if (dashboardData.inspectionsByStatus && dashboardData.inspectionsByStatus.length > 0) {
    doc.setFontSize(14);
    doc.setFont('helvetica', 'bold');
    doc.text('Status Distribution', 20, yPosition);
    yPosition += 10;

    const statusData = dashboardData.inspectionsByStatus.map(item => [
      item.statusName,
      item.count.toString(),
      `${item.percentage}%`
    ]);

    doc.autoTable({
      head: [['Status', 'Count', 'Percentage']],
      body: statusData,
      startY: yPosition,
      styles: {
        fontSize: 10,
        cellPadding: 3,
      },
      headStyles: {
        fillColor: [52, 152, 219],
        textColor: 255,
        fontStyle: 'bold'
      }
    });

    yPosition = (doc as any).lastAutoTable.finalY + 15;
  }

  // Add type distribution
  if (dashboardData.inspectionsByType && dashboardData.inspectionsByType.length > 0) {
    doc.setFontSize(14);
    doc.setFont('helvetica', 'bold');
    doc.text('Type Distribution', 20, yPosition);
    yPosition += 10;

    const typeData = dashboardData.inspectionsByType.map(item => [
      item.typeName,
      item.count.toString(),
      `${item.percentage}%`
    ]);

    doc.autoTable({
      head: [['Type', 'Count', 'Percentage']],
      body: typeData,
      startY: yPosition,
      styles: {
        fontSize: 10,
        cellPadding: 3,
      },
      headStyles: {
        fillColor: [46, 204, 113],
        textColor: 255,
        fontStyle: 'bold'
      }
    });
  }

  // Save the PDF
  doc.save(`${filename}.pdf`);
};
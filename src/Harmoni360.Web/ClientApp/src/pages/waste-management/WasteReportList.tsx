import React from 'react';
import { useGetWasteReportsQuery } from '../../features/waste-management/wasteApi';

const WasteReportList: React.FC = () => {
  const { data = [], isLoading } = useGetWasteReportsQuery();

  if (isLoading) return <div>Loading...</div>;

  return (
    <div>
      <h2>Waste Reports</h2>
      <ul>
        {data.map((w) => (
          <li key={w.id}>{w.title} - {w.category}</li>
        ))}
      </ul>
    </div>
  );
};

export default WasteReportList;

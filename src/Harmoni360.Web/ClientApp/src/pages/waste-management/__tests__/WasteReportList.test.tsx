import { render } from '@testing-library/react';
import WasteReportList from '../WasteReportList';

test('renders list header', () => {
  const { getByText } = render(<WasteReportList />);
  expect(getByText(/Waste Reports/i)).toBeInTheDocument();
});

import React from 'react';
import { useForm } from 'react-hook-form';
import { useCreateWasteReportMutation } from '../../features/waste-management/wasteApi';

interface FormValues {
  title: string;
  description: string;
  category: string;
  generatedDate: string;
  location: string;
  attachments: FileList;
}

const WasteReportForm: React.FC = () => {
  const { register, handleSubmit } = useForm<FormValues>();
  const [createWasteReport] = useCreateWasteReportMutation();

  const onSubmit = async (data: FormValues) => {
    const form = new FormData();
    form.append('title', data.title);
    form.append('description', data.description);
    form.append('category', data.category);
    form.append('generatedDate', data.generatedDate);
    form.append('location', data.location);
    for (const file of Array.from(data.attachments || [])) {
      form.append('attachments', file);
    }
    await createWasteReport(form);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <div>
        <label>Title</label>
        <input {...register('title', { required: true })} />
      </div>
      <div>
        <label>Description</label>
        <textarea {...register('description', { required: true })} />
      </div>
      <div>
        <label>Category</label>
        <select {...register('category', { required: true })}>
          <option value="Hazardous">Hazardous</option>
          <option value="NonHazardous">Non-Hazardous</option>
          <option value="Recyclable">Recyclable</option>
        </select>
      </div>
      <div>
        <label>Date</label>
        <input type="date" {...register('generatedDate', { required: true })} />
      </div>
      <div>
        <label>Location</label>
        <input {...register('location', { required: true })} />
      </div>
      <div>
        <label>Attachments</label>
        <input type="file" multiple {...register('attachments')} />
      </div>
      <button type="submit">Submit</button>
    </form>
  );
};

export default WasteReportForm;

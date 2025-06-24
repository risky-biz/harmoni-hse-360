import React, { useState } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CSpinner,
  CAlert,
  CListGroup,
  CListGroupItem,
  CBadge,
  CForm,
  CFormTextarea,
  CButton,
  CButtonGroup,
  CFormSelect,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faComment,
  faPlus,
  faTrash,
  faUser,
  faClock,
} from '@fortawesome/free-solid-svg-icons';
import { 
  useGetWasteCommentsQuery, 
  useAddWasteCommentMutation,
  useDeleteWasteCommentMutation 
} from '../../services/wasteReportsApi';
import { formatDateTime, formatRelativeTime } from '../../utils/dateUtils';
import { WasteCommentDto } from '../../types/wasteReports';
import { PermissionGuard } from '../auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';

interface WasteCommentsProps {
  reportId: number;
}

const WasteComments: React.FC<WasteCommentsProps> = ({ reportId }) => {
  const [newComment, setNewComment] = useState('');
  const [commentCategory, setCommentCategory] = useState('General');
  
  const {
    data: comments = [],
    isLoading,
    error,
  } = useGetWasteCommentsQuery(reportId);
  
  const [addComment, { isLoading: isAdding }] = useAddWasteCommentMutation();
  const [deleteComment, { isLoading: isDeleting }] = useDeleteWasteCommentMutation();

  const handleSubmitComment = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!newComment.trim()) return;
    
    try {
      await addComment({
        reportId,
        comment: newComment.trim(),
        category: commentCategory,
      }).unwrap();
      
      setNewComment('');
      setCommentCategory('General');
    } catch (error) {
      console.error('Failed to add comment:', error);
    }
  };

  const handleDeleteComment = async (commentId: number) => {
    if (!window.confirm('Are you sure you want to delete this comment?')) {
      return;
    }
    
    try {
      await deleteComment(commentId).unwrap();
    } catch (error) {
      console.error('Failed to delete comment:', error);
    }
  };

  const getCategoryColor = (category?: string) => {
    switch (category) {
      case 'General':
        return 'secondary';
      case 'StatusUpdate':
        return 'info';
      case 'ComplianceNote':
        return 'warning';
      case 'DisposalUpdate':
        return 'success';
      case 'Correction':
        return 'danger';
      default:
        return 'secondary';
    }
  };

  if (isLoading) {
    return (
      <CCard className="mb-4">
        <CCardHeader>
          <h6 className="mb-0">
            <FontAwesomeIcon icon={faComment} className="me-2" />
            Comments
          </h6>
        </CCardHeader>
        <CCardBody>
          <div className="d-flex justify-content-center p-4">
            <CSpinner size="sm" />
            <span className="ms-2">Loading comments...</span>
          </div>
        </CCardBody>
      </CCard>
    );
  }

  if (error) {
    return (
      <CCard className="mb-4">
        <CCardHeader>
          <h6 className="mb-0">
            <FontAwesomeIcon icon={faComment} className="me-2" />
            Comments
          </h6>
        </CCardHeader>
        <CCardBody>
          <CAlert color="danger">
            Failed to load comments. Please try again.
          </CAlert>
        </CCardBody>
      </CCard>
    );
  }

  return (
    <CCard className="mb-4">
      <CCardHeader className="d-flex justify-content-between align-items-center">
        <h6 className="mb-0">
          <FontAwesomeIcon icon={faComment} className="me-2" />
          Comments ({comments.length})
        </h6>
      </CCardHeader>

      <CCardBody>
        {/* Add New Comment Form */}
        <PermissionGuard 
          module={ModuleType.WasteManagement} 
          permission={PermissionType.Create}
        >
          <CForm onSubmit={handleSubmitComment} className="mb-4">
            <div className="row">
              <div className="col-md-9 mb-3">
                <CFormTextarea
                  rows={3}
                  placeholder="Add a comment..."
                  value={newComment}
                  onChange={(e) => setNewComment(e.target.value)}
                  required
                />
              </div>
              <div className="col-md-3 mb-3">
                <CFormSelect
                  value={commentCategory}
                  onChange={(e) => setCommentCategory(e.target.value)}
                  className="mb-2"
                >
                  <option value="General">General</option>
                  <option value="StatusUpdate">Status Update</option>
                  <option value="ComplianceNote">Compliance Note</option>
                  <option value="DisposalUpdate">Disposal Update</option>
                  <option value="Correction">Correction</option>
                </CFormSelect>
                <CButton
                  type="submit"
                  color="primary"
                  disabled={!newComment.trim() || isAdding}
                  className="w-100"
                >
                  {isAdding ? (
                    <CSpinner size="sm" className="me-1" />
                  ) : (
                    <FontAwesomeIcon icon={faPlus} className="me-1" />
                  )}
                  Add Comment
                </CButton>
              </div>
            </div>
          </CForm>
        </PermissionGuard>

        {/* Comments List */}
        {comments.length === 0 ? (
          <div className="text-center text-muted py-4">
            <FontAwesomeIcon
              icon={faComment}
              size="2x"
              className="mb-2 opacity-50"
            />
            <p className="mb-0">No comments yet</p>
          </div>
        ) : (
          <CListGroup flush>
            {comments.map((comment) => (
              <CListGroupItem key={comment.id} className="border-0 px-0">
                <div className="d-flex align-items-start">
                  <div className="me-3 mt-1">
                    <div
                      className="rounded-circle bg-secondary d-flex align-items-center justify-content-center"
                      style={{ width: '40px', height: '40px' }}
                    >
                      <FontAwesomeIcon
                        icon={faUser}
                        className="text-white"
                        size="sm"
                      />
                    </div>
                  </div>

                  <div className="flex-grow-1">
                    <div className="d-flex justify-content-between align-items-start mb-2">
                      <div>
                        <strong className="text-dark me-2">
                          {comment.commentedBy}
                        </strong>
                        {comment.category && (
                          <CBadge
                            color={getCategoryColor(comment.category)}
                            className="me-2"
                          >
                            {comment.category}
                          </CBadge>
                        )}
                        {comment.isInternal && (
                          <CBadge color="warning">Internal</CBadge>
                        )}
                      </div>
                      <div className="d-flex align-items-center">
                        <small className="text-muted me-2">
                          <FontAwesomeIcon icon={faClock} className="me-1" />
                          {formatRelativeTime(comment.commentedAt)}
                        </small>
                        <PermissionGuard 
                          module={ModuleType.WasteManagement} 
                          permission={PermissionType.Delete}
                        >
                          <CButton
                            color="danger"
                            variant="ghost"
                            size="sm"
                            onClick={() => handleDeleteComment(comment.id)}
                            disabled={isDeleting}
                            title="Delete comment"
                          >
                            <FontAwesomeIcon icon={faTrash} />
                          </CButton>
                        </PermissionGuard>
                      </div>
                    </div>

                    <div className="text-dark">
                      {comment.comment}
                    </div>

                    <small className="text-muted">
                      {formatDateTime(comment.commentedAt)}
                    </small>
                  </div>
                </div>
              </CListGroupItem>
            ))}
          </CListGroup>
        )}
      </CCardBody>
    </CCard>
  );
};

export default WasteComments;
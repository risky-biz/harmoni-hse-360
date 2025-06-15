import React, { useState, useEffect } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CTable,
  CTableBody,
  CTableDataCell,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
  CBadge,
  CButton,
  CSpinner,
  CAlert,
  CInputGroup,
  CFormInput,
  CFormSelect,
  CButtonGroup,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faUser,
  faUserPlus,
  faEdit,
  faTrash,
  faSearch,
  faFilter,
  faEllipsisV,
  faShieldAlt,
  faEye,
} from '@fortawesome/free-solid-svg-icons';

// Mock user data (replace with API call)
const mockUsers = [
  {
    id: 1,
    name: 'System Administrator',
    email: 'admin@harmoni360.com',
    role: 'SuperAdmin',
    department: 'IT',
    position: 'System Administrator',
    isActive: true,
    lastLogin: '2024-01-15T10:30:00',
    createdAt: '2024-01-01T00:00:00',
  },
  {
    id: 2,
    name: 'Developer User',
    email: 'developer@harmoni360.com',
    role: 'Developer',
    department: 'IT',
    position: 'Software Developer',
    isActive: true,
    lastLogin: '2024-01-15T09:15:00',
    createdAt: '2024-01-01T00:00:00',
  },
  {
    id: 3,
    name: 'Admin User',
    email: 'admin.user@harmoni360.com',
    role: 'Admin',
    department: 'Management',
    position: 'System Administrator',
    isActive: true,
    lastLogin: '2024-01-15T08:45:00',
    createdAt: '2024-01-01T00:00:00',
  },
  {
    id: 4,
    name: 'Security Manager',
    email: 'security.manager@harmoni360.com',
    role: 'Admin',
    department: 'Security',
    position: 'Security Manager',
    isActive: true,
    lastLogin: '2024-01-14T16:20:00',
    createdAt: '2024-01-01T00:00:00',
  },
  {
    id: 5,
    name: 'Incident Manager',
    email: 'incident.manager@harmoni360.com',
    role: 'IncidentManager',
    department: 'Safety',
    position: 'Incident Response Manager',
    isActive: true,
    lastLogin: '2024-01-14T14:30:00',
    createdAt: '2024-01-01T00:00:00',
  },
  {
    id: 6,
    name: 'Risk Manager',
    email: 'risk.manager@harmoni360.com',
    role: 'RiskManager',
    department: 'Safety',
    position: 'Risk Assessment Manager',
    isActive: true,
    lastLogin: '2024-01-13T11:15:00',
    createdAt: '2024-01-01T00:00:00',
  },
  {
    id: 7,
    name: 'PPE Manager',
    email: 'ppe.manager@harmoni360.com',
    role: 'PPEManager',
    department: 'Safety',
    position: 'PPE Coordinator',
    isActive: true,
    lastLogin: '2024-01-12T13:45:00',
    createdAt: '2024-01-01T00:00:00',
  },
  {
    id: 8,
    name: 'Health Monitor',
    email: 'health.monitor@harmoni360.com',
    role: 'HealthMonitor',
    department: 'Medical',
    position: 'Health & Safety Officer',
    isActive: true,
    lastLogin: '2024-01-11T10:20:00',
    createdAt: '2024-01-01T00:00:00',
  },
  {
    id: 9,
    name: 'Safety Reporter',
    email: 'reporter@harmoni360.com',
    role: 'Reporter',
    department: 'Operations',
    position: 'Safety Reporter',
    isActive: true,
    lastLogin: '2024-01-10T15:30:00',
    createdAt: '2024-01-01T00:00:00',
  },
  {
    id: 10,
    name: 'Viewer User',
    email: 'viewer@harmoni360.com',
    role: 'Viewer',
    department: 'General',
    position: 'General User',
    isActive: true,
    lastLogin: '2024-01-09T12:15:00',
    createdAt: '2024-01-01T00:00:00',
  },
];

interface User {
  id: number;
  name: string;
  email: string;
  role: string;
  department: string;
  position: string;
  isActive: boolean;
  lastLogin: string;
  createdAt: string;
}

const UserManagement: React.FC = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [roleFilter, setRoleFilter] = useState('All');
  const [departmentFilter, setDepartmentFilter] = useState('All');

  useEffect(() => {
    // Simulate API call
    setTimeout(() => {
      setUsers(mockUsers);
      setLoading(false);
    }, 1000);
  }, []);

  const getRoleBadgeColor = (role: string) => {
    const roleColors: { [key: string]: string } = {
      SuperAdmin: 'danger',
      Developer: 'warning',
      Admin: 'primary',
      IncidentManager: 'info',
      RiskManager: 'info',
      PPEManager: 'info',
      HealthMonitor: 'info',
      Reporter: 'success',
      Viewer: 'secondary',
    };
    return roleColors[role] || 'secondary';
  };

  const getStatusBadge = (isActive: boolean) => {
    return (
      <CBadge color={isActive ? 'success' : 'danger'}>
        {isActive ? 'Active' : 'Inactive'}
      </CBadge>
    );
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  const filteredUsers = users.filter((user) => {
    const matchesSearch = user.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         user.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         user.department.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesRole = roleFilter === 'All' || user.role === roleFilter;
    const matchesDepartment = departmentFilter === 'All' || user.department === departmentFilter;
    
    return matchesSearch && matchesRole && matchesDepartment;
  });

  const uniqueRoles = [...new Set(users.map(user => user.role))];
  const uniqueDepartments = [...new Set(users.map(user => user.department))];

  if (loading) {
    return (
      <div className="d-flex justify-content-center align-items-center min-vh-50">
        <CSpinner color="primary" />
      </div>
    );
  }

  return (
    <CRow>
      <CCol xs={12}>
        <CCard className="mb-4">
          <CCardHeader>
            <div className="d-flex justify-content-between align-items-center">
              <div>
                <h4 className="mb-0">
                  <FontAwesomeIcon icon={faUser} className="me-2" />
                  User Management
                </h4>
                <small className="text-medium-emphasis">
                  Manage system users and their permissions
                </small>
              </div>
              <CButton color="primary" onClick={() => alert('Create User functionality coming soon!')}>
                <FontAwesomeIcon icon={faUserPlus} className="me-2" />
                Add User
              </CButton>
            </div>
          </CCardHeader>

          <CCardBody>
            <CAlert color="info" className="mb-4">
              <strong>Demo Users Available:</strong> You can use the credentials shown below to test different user roles and permissions in the system.
            </CAlert>

            {/* Search and Filters */}
            <CRow className="mb-4">
              <CCol md={6}>
                <CInputGroup>
                  <CFormInput
                    placeholder="Search users..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                  />
                  <CButton type="button" color="outline-secondary">
                    <FontAwesomeIcon icon={faSearch} />
                  </CButton>
                </CInputGroup>
              </CCol>
              <CCol md={3}>
                <CFormSelect
                  value={roleFilter}
                  onChange={(e) => setRoleFilter(e.target.value)}
                >
                  <option value="All">All Roles</option>
                  {uniqueRoles.map(role => (
                    <option key={role} value={role}>{role}</option>
                  ))}
                </CFormSelect>
              </CCol>
              <CCol md={3}>
                <CFormSelect
                  value={departmentFilter}
                  onChange={(e) => setDepartmentFilter(e.target.value)}
                >
                  <option value="All">All Departments</option>
                  {uniqueDepartments.map(dept => (
                    <option key={dept} value={dept}>{dept}</option>
                  ))}
                </CFormSelect>
              </CCol>
            </CRow>

            {/* Users Table */}
            <CTable align="middle" className="mb-0 border" hover responsive>
              <CTableHead color="light">
                <CTableRow>
                  <CTableHeaderCell>User</CTableHeaderCell>
                  <CTableHeaderCell>Role</CTableHeaderCell>
                  <CTableHeaderCell>Department</CTableHeaderCell>
                  <CTableHeaderCell>Status</CTableHeaderCell>
                  <CTableHeaderCell>Last Login</CTableHeaderCell>
                  <CTableHeaderCell>Created</CTableHeaderCell>
                  <CTableHeaderCell>Actions</CTableHeaderCell>
                </CTableRow>
              </CTableHead>
              <CTableBody>
                {filteredUsers.map((user) => (
                  <CTableRow key={user.id}>
                    <CTableDataCell>
                      <div>
                        <div className="fw-semibold">{user.name}</div>
                        <div className="small text-medium-emphasis">
                          {user.email}
                        </div>
                        <div className="small text-medium-emphasis">
                          {user.position}
                        </div>
                      </div>
                    </CTableDataCell>
                    <CTableDataCell>
                      <CBadge color={getRoleBadgeColor(user.role)}>
                        {user.role}
                      </CBadge>
                    </CTableDataCell>
                    <CTableDataCell>{user.department}</CTableDataCell>
                    <CTableDataCell>
                      {getStatusBadge(user.isActive)}
                    </CTableDataCell>
                    <CTableDataCell>
                      <small>{formatDateTime(user.lastLogin)}</small>
                    </CTableDataCell>
                    <CTableDataCell>
                      <small>{formatDate(user.createdAt)}</small>
                    </CTableDataCell>
                    <CTableDataCell>
                      <CButtonGroup>
                        <CButton 
                          color="info" 
                          variant="outline" 
                          size="sm"
                          onClick={() => alert(`View user ${user.name} details`)}
                        >
                          <FontAwesomeIcon icon={faEye} />
                        </CButton>
                        <CButton 
                          color="warning" 
                          variant="outline" 
                          size="sm"
                          onClick={() => alert(`Edit user ${user.name}`)}
                        >
                          <FontAwesomeIcon icon={faEdit} />
                        </CButton>
                        <CButton 
                          color="primary" 
                          variant="outline" 
                          size="sm"
                          onClick={() => alert(`Manage permissions for ${user.name}`)}
                        >
                          <FontAwesomeIcon icon={faShieldAlt} />
                        </CButton>
                      </CButtonGroup>
                    </CTableDataCell>
                  </CTableRow>
                ))}
              </CTableBody>
            </CTable>

            {filteredUsers.length === 0 && (
              <div className="text-center py-4">
                <p className="text-medium-emphasis">No users found matching your criteria.</p>
              </div>
            )}
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
  );
};

export default UserManagement;
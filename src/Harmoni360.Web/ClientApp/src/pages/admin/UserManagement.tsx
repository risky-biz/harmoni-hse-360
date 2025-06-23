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
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CForm,
  CFormLabel,
  CFormCheck,
  CToast,
  CToastBody,
  CToastHeader,
  CToaster,
  CPagination,
  CPaginationItem,
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
  faUnlock,
  faKey,
  faHistory,
  faMapMarkerAlt,
  faPhone,
  faEnvelope,
  faBuilding,
  faCalendar,
  faUserTie,
} from '@fortawesome/free-solid-svg-icons';
import { userManagementApi } from '../../services/userManagementApi';
import { UserStatus } from '../../types/enums';


interface User {
  id: number;
  name: string;
  email: string;
  employeeId: string;
  department: string;
  position: string;
  isActive: boolean;
  status: UserStatus;
  phoneNumber?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  supervisorEmployeeId?: string;
  hireDate?: string;
  workLocation?: string;
  costCenter?: string;
  requiresMFA: boolean;
  lastPasswordChange?: string;
  lastLoginAt?: string;
  failedLoginAttempts: number;
  accountLockedUntil?: string;
  preferredLanguage?: string;
  timeZone?: string;
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
  roles: UserRole[];
}

interface UserRole {
  roleId: number;
  roleName: string;
  roleType: string;
  description: string;
  assignedAt: string;
}

interface CreateUserFormData {
  email: string;
  name: string;
  employeeId: string;
  department: string;
  position: string;
  password: string;
  phoneNumber?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  supervisorEmployeeId?: string;
  hireDate?: string;
  workLocation?: string;
  costCenter?: string;
  preferredLanguage?: string;
  timeZone?: string;
  roleIds: number[];
}

interface UpdateUserFormData {
  name: string;
  department: string;
  position: string;
  isActive: boolean;
  phoneNumber?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  supervisorEmployeeId?: string;
  hireDate?: string;
  workLocation?: string;
  costCenter?: string;
  preferredLanguage?: string;
  timeZone?: string;
  requiresMFA: boolean;
  status: UserStatus;
  roleIds: number[];
}

const UserManagement: React.FC = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [roleFilter, setRoleFilter] = useState('All');
  const [departmentFilter, setDepartmentFilter] = useState('All');
  const [locationFilter, setLocationFilter] = useState('All');
  const [statusFilter, setStatusFilter] = useState<UserStatus | 'All'>('All');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(20);
  const [totalPages, setTotalPages] = useState(0);
  const [totalUsers, setTotalUsers] = useState(0);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showUserModal, setShowUserModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [toasts, setToasts] = useState<any[]>([]);
  const [availableRoles, setAvailableRoles] = useState<any[]>([]);
  const [createUserForm, setCreateUserForm] = useState<CreateUserFormData>({
    email: '',
    name: '',
    employeeId: '',
    department: '',
    position: '',
    password: '',
    phoneNumber: '',
    emergencyContactName: '',
    emergencyContactPhone: '',
    supervisorEmployeeId: '',
    hireDate: '',
    workLocation: '',
    costCenter: '',
    preferredLanguage: 'en',
    timeZone: 'Asia/Jakarta',
    roleIds: [],
  });
  const [isCreating, setIsCreating] = useState(false);
  const [passwordError, setPasswordError] = useState('');
  const [editUserForm, setEditUserForm] = useState<UpdateUserFormData>({
    name: '',
    department: '',
    position: '',
    isActive: true,
    phoneNumber: '',
    emergencyContactName: '',
    emergencyContactPhone: '',
    supervisorEmployeeId: '',
    hireDate: '',
    workLocation: '',
    costCenter: '',
    preferredLanguage: 'en',
    timeZone: 'Asia/Jakarta',
    requiresMFA: false,
    status: UserStatus.Active,
    roleIds: [],
  });
  const [isUpdating, setIsUpdating] = useState(false);

  const generatePassword = (): string => {
    const uppercase = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    const lowercase = 'abcdefghijklmnopqrstuvwxyz';
    const numbers = '0123456789';
    const special = '!@#$%^&*';
    
    let password = '';
    password += uppercase[Math.floor(Math.random() * uppercase.length)];
    password += lowercase[Math.floor(Math.random() * lowercase.length)];
    password += numbers[Math.floor(Math.random() * numbers.length)];
    password += special[Math.floor(Math.random() * special.length)];
    
    // Add 4 more random characters
    const all = uppercase + lowercase + numbers + special;
    for (let i = 0; i < 4; i++) {
      password += all[Math.floor(Math.random() * all.length)];
    }
    
    // Shuffle the password
    return password.split('').sort(() => Math.random() - 0.5).join('');
  };

  const validatePassword = (password: string): boolean => {
    if (password.length < 8) {
      setPasswordError('Password must be at least 8 characters long');
      return false;
    }
    
    const hasUpperCase = /[A-Z]/.test(password);
    const hasLowerCase = /[a-z]/.test(password);
    const hasNumber = /[0-9]/.test(password);
    const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(password);
    
    if (!hasUpperCase || !hasLowerCase || !hasNumber || !hasSpecialChar) {
      setPasswordError('Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character');
      return false;
    }
    
    setPasswordError('');
    return true;
  };

  useEffect(() => {
    loadUsers();
    loadRoles();
  }, [currentPage, searchTerm, roleFilter, departmentFilter, locationFilter, statusFilter]);

  const loadUsers = async () => {
    try {
      setLoading(true);
      const response = await userManagementApi.getUsers({
        page: currentPage,
        pageSize,
        searchTerm: searchTerm || undefined,
        department: departmentFilter !== 'All' ? departmentFilter : undefined,
        workLocation: locationFilter !== 'All' ? locationFilter : undefined,
        status: statusFilter !== 'All' ? statusFilter : undefined,
        roleId: roleFilter !== 'All' ? parseInt(roleFilter) : undefined,
      });
      setUsers(response.users);
      setTotalPages(response.totalPages);
      setTotalUsers(response.totalCount);
    } catch (error) {
      console.error('Error loading users:', error);
      addToast('Error loading users', 'danger');
    } finally {
      setLoading(false);
    }
  };

  const loadRoles = async () => {
    try {
      const roles = await userManagementApi.getRoles();
      setAvailableRoles(roles);
    } catch (error) {
      console.error('Error loading roles:', error);
    }
  };

  const addToast = (message: string, color: string = 'info') => {
    const toast = {
      id: Date.now(),
      message,
      color,
    };
    setToasts(prev => [...prev, toast]);
    setTimeout(() => {
      setToasts(prev => prev.filter(t => t.id !== toast.id));
    }, 5000);
  };

  const handleCreateUser = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!createUserForm.email || !createUserForm.name || !createUserForm.employeeId || 
        !createUserForm.department || !createUserForm.position || !createUserForm.password) {
      addToast('Please fill in all required fields', 'danger');
      return;
    }

    if (!validatePassword(createUserForm.password)) {
      addToast(passwordError, 'danger');
      return;
    }

    if (createUserForm.roleIds.length === 0) {
      addToast('Please select at least one role', 'danger');
      return;
    }

    try {
      setIsCreating(true);
      await userManagementApi.createUser(createUserForm);
      addToast('User created successfully', 'success');
      setShowCreateModal(false);
      setCreateUserForm({
        email: '',
        name: '',
        employeeId: '',
        department: '',
        position: '',
        password: '',
        phoneNumber: '',
        emergencyContactName: '',
        emergencyContactPhone: '',
        supervisorEmployeeId: '',
        hireDate: '',
        workLocation: '',
        costCenter: '',
        preferredLanguage: 'en',
        timeZone: 'Asia/Jakarta',
        roleIds: [],
      });
      loadUsers();
    } catch (error: any) {
      console.error('Error creating user:', error);
      const errorMessage = error.message || 'Error creating user';
      addToast(errorMessage, 'danger');
    } finally {
      setIsCreating(false);
    }
  };

  const handleEditUser = async (user: User) => {
    try {
      // Fetch the complete user details to ensure we have all HSSE fields
      const fullUserDetails = await userManagementApi.getUserById(user.id);
      
      setSelectedUser(fullUserDetails);
      setEditUserForm({
        name: fullUserDetails.name,
        department: fullUserDetails.department,
        position: fullUserDetails.position,
        isActive: fullUserDetails.isActive,
        phoneNumber: fullUserDetails.phoneNumber || '',
        emergencyContactName: fullUserDetails.emergencyContactName || '',
        emergencyContactPhone: fullUserDetails.emergencyContactPhone || '',
        supervisorEmployeeId: fullUserDetails.supervisorEmployeeId || '',
        hireDate: fullUserDetails.hireDate ? fullUserDetails.hireDate.split('T')[0] : '',
        workLocation: fullUserDetails.workLocation || '',
        costCenter: fullUserDetails.costCenter || '',
        preferredLanguage: fullUserDetails.preferredLanguage || 'en',
        timeZone: fullUserDetails.timeZone || 'Asia/Jakarta',
        requiresMFA: fullUserDetails.requiresMFA,
        status: fullUserDetails.status || UserStatus.Active,
        roleIds: fullUserDetails.roles.map(r => r.roleId),
      });
      setShowEditModal(true);
    } catch (error) {
      console.error('Error fetching user details:', error);
      addToast('Error loading user details', 'danger');
    }
  };

  const handleUpdateUser = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!selectedUser) return;
    
    if (!editUserForm.name || !editUserForm.department || !editUserForm.position) {
      addToast('Please fill in all required fields', 'danger');
      return;
    }

    if (editUserForm.roleIds.length === 0) {
      addToast('Please select at least one role', 'danger');
      return;
    }

    try {
      setIsUpdating(true);
      await userManagementApi.updateUser(selectedUser.id, editUserForm);
      addToast('User updated successfully', 'success');
      setShowEditModal(false);
      loadUsers();
    } catch (error: any) {
      console.error('Error updating user:', error);
      const errorMessage = error.message || 'Error updating user';
      addToast(errorMessage, 'danger');
    } finally {
      setIsUpdating(false);
    }
  };

  const handleUserStatusChange = async (userId: number, status: UserStatus, reason?: string) => {
    try {
      await userManagementApi.changeUserStatus(userId, status, reason);
      addToast(`User status changed to ${status}`, 'success');
      loadUsers();
    } catch (error) {
      console.error('Error changing user status:', error);
      addToast('Error changing user status', 'danger');
    }
  };

  const handleUnlockUser = async (userId: number, reason?: string) => {
    try {
      await userManagementApi.unlockUser(userId, reason);
      addToast('User account unlocked successfully', 'success');
      loadUsers();
    } catch (error) {
      console.error('Error unlocking user:', error);
      addToast('Error unlocking user account', 'danger');
    }
  };

  const handleResetPassword = async (userId: number, newPassword: string) => {
    try {
      await userManagementApi.resetPassword(userId, newPassword);
      addToast('Password reset successfully', 'success');
    } catch (error) {
      console.error('Error resetting password:', error);
      addToast('Error resetting password', 'danger');
    }
  };

  const getRoleBadgeColor = (roleType: string) => {
    const roleColors: { [key: string]: string } = {
      SuperAdmin: 'danger',
      Developer: 'warning',
      Admin: 'primary',
      SecurityManager: 'dark',
      ComplianceOfficer: 'dark',
      IncidentManager: 'info',
      RiskManager: 'info',
      PPEManager: 'info',
      HealthMonitor: 'info',
      SafetyOfficer: 'secondary',
      DepartmentHead: 'secondary',
      HSEManager: 'primary',
      Reporter: 'success',
      Viewer: 'light',
    };
    return roleColors[roleType] || 'secondary';
  };

  const getStatusFilter = () => {
    return (
      <CFormSelect
        value={statusFilter}
        onChange={(e) => setStatusFilter(e.target.value === 'All' ? 'All' : parseInt(e.target.value) as UserStatus)}
      >
        <option value="All">All Status</option>
        <option value={UserStatus.Active}>Active</option>
        <option value={UserStatus.Inactive}>Inactive</option>
        <option value={UserStatus.Suspended}>Suspended</option>
        <option value={UserStatus.PendingActivation}>Pending Activation</option>
        <option value={UserStatus.Terminated}>Terminated</option>
      </CFormSelect>
    );
  };

  const getStatusBadge = (status: UserStatus, isLocked: boolean = false) => {
    if (isLocked) {
      return <CBadge color="warning">Locked</CBadge>;
    }
    
    const statusColors: { [key in UserStatus]: string } = {
      [UserStatus.Active]: 'success',
      [UserStatus.Inactive]: 'secondary',
      [UserStatus.Suspended]: 'warning',
      [UserStatus.PendingActivation]: 'info',
      [UserStatus.Terminated]: 'danger',
    };
    
    return (
      <CBadge color={statusColors[status]}>
        {UserStatus[status]}
      </CBadge>
    );
  };

  const isUserLocked = (user: User) => {
    return user.accountLockedUntil && new Date(user.accountLockedUntil) > new Date();
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  const uniqueDepartments = [...new Set(users.map(user => user.department))];
  const uniqueLocations = [...new Set(users.map(user => user.workLocation).filter(Boolean))];

  const getUserActions = (user: User) => {
    const locked = isUserLocked(user);
    return [
      {
        label: 'View Details',
        icon: faEye,
        color: 'info',
        action: () => {
          setSelectedUser(user);
          setShowUserModal(true);
        },
      },
      {
        label: 'Edit User',
        icon: faEdit,
        color: 'warning',
        action: () => handleEditUser(user),
      },
      ...(locked ? [{
        label: 'Unlock Account',
        icon: faUnlock,
        color: 'success',
        action: () => handleUnlockUser(user.id, 'Manual unlock by admin'),
      }] : []),
      {
        label: 'Reset Password',
        icon: faKey,
        color: 'secondary',
        action: () => {
          const newPassword = prompt('Enter new password:');
          if (newPassword) {
            handleResetPassword(user.id, newPassword);
          }
        },
      },
      {
        label: 'Change Status',
        icon: faUserTie,
        color: 'primary',
        action: () => {
          // Show status change modal or dropdown
          const newStatus = prompt('Enter new status (1=Active, 2=Inactive, 3=Suspended, 5=Terminated):');
          if (newStatus) {
            const status = parseInt(newStatus) as UserStatus;
            const reason = prompt('Reason for status change:');
            handleUserStatusChange(user.id, status, reason || undefined);
          }
        },
      },
    ];
  };

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
              <CButton color="primary" onClick={() => setShowCreateModal(true)}>
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
              <CCol md={4}>
                <CInputGroup>
                  <CFormInput
                    placeholder="Search users by name, email, or employee ID..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                  />
                  <CButton type="button" color="outline-secondary">
                    <FontAwesomeIcon icon={faSearch} />
                  </CButton>
                </CInputGroup>
              </CCol>
              <CCol md={2}>
                <CFormSelect
                  value={roleFilter}
                  onChange={(e) => setRoleFilter(e.target.value)}
                >
                  <option value="All">All Roles</option>
                  {availableRoles.map(role => (
                    <option key={role.id} value={role.id}>{role.name}</option>
                  ))}
                </CFormSelect>
              </CCol>
              <CCol md={2}>
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
              <CCol md={2}>
                <CFormSelect
                  value={locationFilter}
                  onChange={(e) => setLocationFilter(e.target.value)}
                >
                  <option value="All">All Locations</option>
                  {uniqueLocations.map(location => (
                    <option key={location} value={location}>{location}</option>
                  ))}
                </CFormSelect>
              </CCol>
              <CCol md={2}>
                {getStatusFilter()}
              </CCol>
            </CRow>

            {/* Users Table */}
            <CTable align="middle" className="mb-0 border" hover responsive>
              <CTableHead color="light">
                <CTableRow>
                  <CTableHeaderCell className="w-25">User</CTableHeaderCell>
                  <CTableHeaderCell className="w-15">Roles</CTableHeaderCell>
                  <CTableHeaderCell className="w-15">Department</CTableHeaderCell>
                  <CTableHeaderCell className="w-10">Location</CTableHeaderCell>
                  <CTableHeaderCell className="w-10">Status</CTableHeaderCell>
                  <CTableHeaderCell className="w-10">Last Login</CTableHeaderCell>
                  <CTableHeaderCell className="w-15">Actions</CTableHeaderCell>
                </CTableRow>
              </CTableHead>
              <CTableBody>
                {users.map((user) => (
                  <CTableRow key={user.id}>
                    <CTableDataCell>
                      <div>
                        <div className="fw-semibold">{user.name}</div>
                        <div className="small text-medium-emphasis">
                          <FontAwesomeIcon icon={faEnvelope} className="me-1" />
                          {user.email}
                        </div>
                        <div className="small text-medium-emphasis">
                          ID: {user.employeeId}
                        </div>
                        {user.phoneNumber && (
                          <div className="small text-medium-emphasis">
                            <FontAwesomeIcon icon={faPhone} className="me-1" />
                            {user.phoneNumber}
                          </div>
                        )}
                      </div>
                    </CTableDataCell>
                    <CTableDataCell>
                      <div className="d-flex flex-wrap gap-1">
                        {user.roles.slice(0, 2).map((role) => (
                          <CBadge key={role.roleId} color={getRoleBadgeColor(role.roleType)} className="small">
                            {role.roleName}
                          </CBadge>
                        ))}
                        {user.roles.length > 2 && (
                          <CBadge color="light" className="small">+{user.roles.length - 2}</CBadge>
                        )}
                      </div>
                    </CTableDataCell>
                    <CTableDataCell>
                      <div>
                        <div className="fw-medium">{user.department}</div>
                        <div className="small text-medium-emphasis">{user.position}</div>
                      </div>
                    </CTableDataCell>
                    <CTableDataCell>
                      {user.workLocation && (
                        <div className="small">
                          <FontAwesomeIcon icon={faMapMarkerAlt} className="me-1" />
                          {user.workLocation}
                        </div>
                      )}
                    </CTableDataCell>
                    <CTableDataCell>
                      <div className="d-flex flex-column gap-1">
                        {getStatusBadge(user.status, isUserLocked(user))}
                        {user.requiresMFA && (
                          <CBadge color="info" className="small">
                            <FontAwesomeIcon icon={faShieldAlt} className="me-1" />MFA
                          </CBadge>
                        )}
                      </div>
                    </CTableDataCell>
                    <CTableDataCell>
                      <small>
                        {user.lastLoginAt ? formatDateTime(user.lastLoginAt) : 'Never'}
                      </small>
                    </CTableDataCell>
                    <CTableDataCell>
                      <CDropdown>
                        <CDropdownToggle 
                          color="ghost" 
                          size="sm"
                          caret={false}
                        >
                          <FontAwesomeIcon icon={faEllipsisV} />
                        </CDropdownToggle>
                        <CDropdownMenu>
                          {getUserActions(user).map((action, index) => (
                            <CDropdownItem 
                              key={index}
                              onClick={action.action}
                              className={`text-${action.color}`}
                            >
                              <FontAwesomeIcon icon={action.icon} className="me-2" />
                              {action.label}
                            </CDropdownItem>
                          ))}
                        </CDropdownMenu>
                      </CDropdown>
                    </CTableDataCell>
                  </CTableRow>
                ))}
              </CTableBody>
            </CTable>

            {users.length === 0 && !loading && (
              <div className="text-center py-4">
                <p className="text-medium-emphasis">No users found matching your criteria.</p>
              </div>
            )}
            
            {/* Pagination */}
            {totalPages > 1 && (
              <div className="d-flex justify-content-between align-items-center mt-4">
                <div className="small text-medium-emphasis">
                  Showing {((currentPage - 1) * pageSize) + 1} to {Math.min(currentPage * pageSize, totalUsers)} of {totalUsers} users
                </div>
                <CPagination aria-label="User pagination">
                  <CPaginationItem 
                    disabled={currentPage === 1}
                    onClick={() => setCurrentPage(currentPage - 1)}
                  >
                    Previous
                  </CPaginationItem>
                  {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                    const pageNum = Math.max(1, Math.min(totalPages - 4, currentPage - 2)) + i;
                    return (
                      <CPaginationItem
                        key={pageNum}
                        active={pageNum === currentPage}
                        onClick={() => setCurrentPage(pageNum)}
                      >
                        {pageNum}
                      </CPaginationItem>
                    );
                  })}
                  <CPaginationItem 
                    disabled={currentPage === totalPages}
                    onClick={() => setCurrentPage(currentPage + 1)}
                  >
                    Next
                  </CPaginationItem>
                </CPagination>
              </div>
            )}
          </CCardBody>
        </CCard>
      </CCol>
      
      {/* Create User Modal */}
      <CModal visible={showCreateModal} onClose={() => setShowCreateModal(false)} size="lg">
        <CForm onSubmit={handleCreateUser}>
          <CModalHeader>
            <CModalTitle>Create New User</CModalTitle>
          </CModalHeader>
          <CModalBody>
            <CRow>
              <CCol md={6}>
                <h6 className="mb-3">Basic Information</h6>
                <div className="mb-3">
                  <CFormLabel htmlFor="email">Email *</CFormLabel>
                  <CFormInput
                    type="email"
                    id="email"
                    value={createUserForm.email}
                    onChange={(e) => setCreateUserForm({...createUserForm, email: e.target.value})}
                    required
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="name">Full Name *</CFormLabel>
                  <CFormInput
                    type="text"
                    id="name"
                    value={createUserForm.name}
                    onChange={(e) => setCreateUserForm({...createUserForm, name: e.target.value})}
                    required
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="employeeId">Employee ID *</CFormLabel>
                  <CFormInput
                    type="text"
                    id="employeeId"
                    value={createUserForm.employeeId}
                    onChange={(e) => setCreateUserForm({...createUserForm, employeeId: e.target.value})}
                    required
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="password">Password *</CFormLabel>
                  <CInputGroup>
                    <CFormInput
                      type="password"
                      id="password"
                      value={createUserForm.password}
                      onChange={(e) => {
                        setCreateUserForm({...createUserForm, password: e.target.value});
                        if (e.target.value) {
                          validatePassword(e.target.value);
                        } else {
                          setPasswordError('');
                        }
                      }}
                      invalid={!!passwordError && !!createUserForm.password}
                      required
                    />
                    <CButton
                      type="button"
                      color="outline-secondary"
                      onClick={() => {
                        const newPassword = generatePassword();
                        setCreateUserForm({...createUserForm, password: newPassword});
                        validatePassword(newPassword);
                        // Copy to clipboard
                        navigator.clipboard.writeText(newPassword);
                        addToast('Password generated and copied to clipboard', 'success');
                      }}
                    >
                      Generate
                    </CButton>
                  </CInputGroup>
                  {passwordError && createUserForm.password && (
                    <div className="invalid-feedback d-block">{passwordError}</div>
                  )}
                  <small className="text-muted">
                    Password must be at least 8 characters with uppercase, lowercase, number, and special character
                  </small>
                </div>
              </CCol>
              <CCol md={6}>
                <h6 className="mb-3">Employment Details</h6>
                <div className="mb-3">
                  <CFormLabel htmlFor="department">Department *</CFormLabel>
                  <CFormInput
                    type="text"
                    id="department"
                    value={createUserForm.department}
                    onChange={(e) => setCreateUserForm({...createUserForm, department: e.target.value})}
                    required
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="position">Position *</CFormLabel>
                  <CFormInput
                    type="text"
                    id="position"
                    value={createUserForm.position}
                    onChange={(e) => setCreateUserForm({...createUserForm, position: e.target.value})}
                    required
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="workLocation">Work Location</CFormLabel>
                  <CFormInput
                    type="text"
                    id="workLocation"
                    value={createUserForm.workLocation || ''}
                    onChange={(e) => setCreateUserForm({...createUserForm, workLocation: e.target.value})}
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="hireDate">Hire Date</CFormLabel>
                  <CFormInput
                    type="date"
                    id="hireDate"
                    value={createUserForm.hireDate || ''}
                    onChange={(e) => setCreateUserForm({...createUserForm, hireDate: e.target.value})}
                  />
                </div>
              </CCol>
              <CCol md={6}>
                <h6 className="mb-3 mt-3">Contact Information</h6>
                <div className="mb-3">
                  <CFormLabel htmlFor="phoneNumber">Phone Number</CFormLabel>
                  <CFormInput
                    type="tel"
                    id="phoneNumber"
                    value={createUserForm.phoneNumber || ''}
                    onChange={(e) => setCreateUserForm({...createUserForm, phoneNumber: e.target.value})}
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="supervisorEmployeeId">Supervisor Employee ID</CFormLabel>
                  <CFormInput
                    type="text"
                    id="supervisorEmployeeId"
                    value={createUserForm.supervisorEmployeeId || ''}
                    onChange={(e) => setCreateUserForm({...createUserForm, supervisorEmployeeId: e.target.value})}
                  />
                </div>
              </CCol>
              <CCol md={6}>
                <h6 className="mb-3 mt-3">Emergency Contact</h6>
                <div className="mb-3">
                  <CFormLabel htmlFor="emergencyContactName">Emergency Contact Name</CFormLabel>
                  <CFormInput
                    type="text"
                    id="emergencyContactName"
                    value={createUserForm.emergencyContactName || ''}
                    onChange={(e) => setCreateUserForm({...createUserForm, emergencyContactName: e.target.value})}
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="emergencyContactPhone">Emergency Contact Phone</CFormLabel>
                  <CFormInput
                    type="tel"
                    id="emergencyContactPhone"
                    value={createUserForm.emergencyContactPhone || ''}
                    onChange={(e) => setCreateUserForm({...createUserForm, emergencyContactPhone: e.target.value})}
                  />
                </div>
              </CCol>
              <CCol md={12}>
                <h6 className="mb-3 mt-3">Role Assignment *</h6>
                <div className="mb-3">
                  <CFormLabel>Select Roles (at least one required)</CFormLabel>
                  <div className="d-flex flex-wrap gap-2">
                    {availableRoles.map((role) => (
                      <CFormCheck
                        key={role.id}
                        id={`role-${role.id}`}
                        label={role.name}
                        checked={createUserForm.roleIds.includes(role.id)}
                        onChange={(e) => {
                          if (e.target.checked) {
                            setCreateUserForm({
                              ...createUserForm,
                              roleIds: [...createUserForm.roleIds, role.id]
                            });
                          } else {
                            setCreateUserForm({
                              ...createUserForm,
                              roleIds: createUserForm.roleIds.filter(id => id !== role.id)
                            });
                          }
                        }}
                      />
                    ))}
                  </div>
                </div>
              </CCol>
            </CRow>
          </CModalBody>
          <CModalFooter>
            <CButton 
              color="secondary" 
              onClick={() => {
                setShowCreateModal(false);
                setCreateUserForm({
                  email: '',
                  name: '',
                  employeeId: '',
                  department: '',
                  position: '',
                  password: '',
                  phoneNumber: '',
                  emergencyContactName: '',
                  emergencyContactPhone: '',
                  supervisorEmployeeId: '',
                  hireDate: '',
                  workLocation: '',
                  costCenter: '',
                  preferredLanguage: 'en',
                  timeZone: 'Asia/Jakarta',
                  roleIds: [],
                });
              }}
            >
              Cancel
            </CButton>
            <CButton 
              color="primary" 
              type="submit"
              disabled={isCreating}
            >
              {isCreating ? 'Creating...' : 'Create User'}
            </CButton>
          </CModalFooter>
        </CForm>
      </CModal>
      
      {/* Edit User Modal */}
      <CModal visible={showEditModal} onClose={() => setShowEditModal(false)} size="xl">
        <CForm onSubmit={handleUpdateUser}>
          <CModalHeader>
            <CModalTitle>Edit User: {selectedUser?.name}</CModalTitle>
          </CModalHeader>
          <CModalBody>
            <CRow>
              <CCol md={6}>
                <h6 className="mb-3">Basic Information</h6>
                <div className="mb-3">
                  <CFormLabel>Email (Read-only)</CFormLabel>
                  <CFormInput
                    type="email"
                    value={selectedUser?.email || ''}
                    disabled
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel>Employee ID (Read-only)</CFormLabel>
                  <CFormInput
                    type="text"
                    value={selectedUser?.employeeId || ''}
                    disabled
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="edit-name">Full Name *</CFormLabel>
                  <CFormInput
                    type="text"
                    id="edit-name"
                    value={editUserForm.name}
                    onChange={(e) => setEditUserForm({...editUserForm, name: e.target.value})}
                    required
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="edit-status">Status *</CFormLabel>
                  <CFormSelect
                    id="edit-status"
                    value={editUserForm.status}
                    onChange={(e) => setEditUserForm({...editUserForm, status: parseInt(e.target.value) as UserStatus})}
                    required
                  >
                    <option value={UserStatus.Active}>Active</option>
                    <option value={UserStatus.Inactive}>Inactive</option>
                    <option value={UserStatus.Suspended}>Suspended</option>
                    <option value={UserStatus.PendingActivation}>Pending Activation</option>
                    <option value={UserStatus.Terminated}>Terminated</option>
                  </CFormSelect>
                </div>
                <div className="mb-3">
                  <CFormCheck
                    id="edit-active"
                    label="User is Active"
                    checked={editUserForm.isActive}
                    onChange={(e) => setEditUserForm({...editUserForm, isActive: e.target.checked})}
                  />
                </div>
                <div className="mb-3">
                  <CFormCheck
                    id="edit-mfa"
                    label="Require Multi-Factor Authentication (MFA)"
                    checked={editUserForm.requiresMFA}
                    onChange={(e) => setEditUserForm({...editUserForm, requiresMFA: e.target.checked})}
                  />
                </div>
              </CCol>
              <CCol md={6}>
                <h6 className="mb-3">Employment Details</h6>
                <div className="mb-3">
                  <CFormLabel htmlFor="edit-department">Department *</CFormLabel>
                  <CFormInput
                    type="text"
                    id="edit-department"
                    value={editUserForm.department}
                    onChange={(e) => setEditUserForm({...editUserForm, department: e.target.value})}
                    required
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="edit-position">Position *</CFormLabel>
                  <CFormInput
                    type="text"
                    id="edit-position"
                    value={editUserForm.position}
                    onChange={(e) => setEditUserForm({...editUserForm, position: e.target.value})}
                    required
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="edit-workLocation">Work Location</CFormLabel>
                  <CFormInput
                    type="text"
                    id="edit-workLocation"
                    value={editUserForm.workLocation || ''}
                    onChange={(e) => setEditUserForm({...editUserForm, workLocation: e.target.value})}
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="edit-hireDate">Hire Date</CFormLabel>
                  <CFormInput
                    type="date"
                    id="edit-hireDate"
                    value={editUserForm.hireDate || ''}
                    onChange={(e) => setEditUserForm({...editUserForm, hireDate: e.target.value})}
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="edit-costCenter">Cost Center</CFormLabel>
                  <CFormInput
                    type="text"
                    id="edit-costCenter"
                    value={editUserForm.costCenter || ''}
                    onChange={(e) => setEditUserForm({...editUserForm, costCenter: e.target.value})}
                  />
                </div>
              </CCol>
              <CCol md={6}>
                <h6 className="mb-3 mt-3">Contact Information</h6>
                <div className="mb-3">
                  <CFormLabel htmlFor="edit-phoneNumber">Phone Number</CFormLabel>
                  <CFormInput
                    type="tel"
                    id="edit-phoneNumber"
                    value={editUserForm.phoneNumber || ''}
                    onChange={(e) => setEditUserForm({...editUserForm, phoneNumber: e.target.value})}
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="edit-supervisorEmployeeId">Supervisor Employee ID</CFormLabel>
                  <CFormInput
                    type="text"
                    id="edit-supervisorEmployeeId"
                    value={editUserForm.supervisorEmployeeId || ''}
                    onChange={(e) => setEditUserForm({...editUserForm, supervisorEmployeeId: e.target.value})}
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="edit-preferredLanguage">Preferred Language</CFormLabel>
                  <CFormSelect
                    id="edit-preferredLanguage"
                    value={editUserForm.preferredLanguage || 'en'}
                    onChange={(e) => setEditUserForm({...editUserForm, preferredLanguage: e.target.value})}
                  >
                    <option value="en">English</option>
                    <option value="id">Indonesian</option>
                  </CFormSelect>
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="edit-timeZone">Time Zone</CFormLabel>
                  <CFormSelect
                    id="edit-timeZone"
                    value={editUserForm.timeZone || 'Asia/Jakarta'}
                    onChange={(e) => setEditUserForm({...editUserForm, timeZone: e.target.value})}
                  >
                    <option value="Asia/Jakarta">Asia/Jakarta (WIB)</option>
                    <option value="Asia/Makassar">Asia/Makassar (WITA)</option>
                    <option value="Asia/Jayapura">Asia/Jayapura (WIT)</option>
                  </CFormSelect>
                </div>
              </CCol>
              <CCol md={6}>
                <h6 className="mb-3 mt-3">Emergency Contact</h6>
                <div className="mb-3">
                  <CFormLabel htmlFor="edit-emergencyContactName">Emergency Contact Name</CFormLabel>
                  <CFormInput
                    type="text"
                    id="edit-emergencyContactName"
                    value={editUserForm.emergencyContactName || ''}
                    onChange={(e) => setEditUserForm({...editUserForm, emergencyContactName: e.target.value})}
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="edit-emergencyContactPhone">Emergency Contact Phone</CFormLabel>
                  <CFormInput
                    type="tel"
                    id="edit-emergencyContactPhone"
                    value={editUserForm.emergencyContactPhone || ''}
                    onChange={(e) => setEditUserForm({...editUserForm, emergencyContactPhone: e.target.value})}
                  />
                </div>
              </CCol>
              <CCol md={12}>
                <h6 className="mb-3 mt-3">Role Assignment *</h6>
                <div className="mb-3">
                  <CFormLabel>Select Roles (at least one required)</CFormLabel>
                  <CRow>
                    {availableRoles.map((role) => (
                      <CCol md={4} key={role.id} className="mb-3">
                        <div className={`p-2 border rounded ${editUserForm.roleIds.includes(role.id) ? 'border-primary bg-light' : 'border-secondary'}`}>
                          <CFormCheck
                            id={`edit-role-${role.id}`}
                            label={
                              <div>
                                <strong className={editUserForm.roleIds.includes(role.id) ? 'text-primary' : ''}>{role.name}</strong>
                                <br />
                                <small className="text-muted">{role.description}</small>
                              </div>
                            }
                            checked={editUserForm.roleIds.includes(role.id)}
                            onChange={(e) => {
                              if (e.target.checked) {
                                setEditUserForm({
                                  ...editUserForm,
                                  roleIds: [...editUserForm.roleIds, role.id]
                                });
                              } else {
                                setEditUserForm({
                                  ...editUserForm,
                                  roleIds: editUserForm.roleIds.filter(id => id !== role.id)
                                });
                              }
                            }}
                          />
                        </div>
                      </CCol>
                    ))}
                  </CRow>
                </div>
              </CCol>
              {selectedUser && (
                <CCol md={12}>
                  <h6 className="mb-3 mt-3">User Information</h6>
                  <div className="text-muted small">
                    <div>Created: {formatDateTime(selectedUser.createdAt)} by {selectedUser.createdBy}</div>
                    {selectedUser.lastModifiedAt && (
                      <div>Last Modified: {formatDateTime(selectedUser.lastModifiedAt)} by {selectedUser.lastModifiedBy}</div>
                    )}
                    {selectedUser.lastLoginAt && (
                      <div>Last Login: {formatDateTime(selectedUser.lastLoginAt)}</div>
                    )}
                    {selectedUser.lastPasswordChange && (
                      <div>Last Password Change: {formatDateTime(selectedUser.lastPasswordChange)}</div>
                    )}
                  </div>
                </CCol>
              )}
            </CRow>
          </CModalBody>
          <CModalFooter>
            <CButton 
              color="secondary" 
              onClick={() => {
                setShowEditModal(false);
                setSelectedUser(null);
              }}
            >
              Cancel
            </CButton>
            <CButton 
              color="primary" 
              type="submit"
              disabled={isUpdating}
            >
              {isUpdating ? 'Updating...' : 'Update User'}
            </CButton>
          </CModalFooter>
        </CForm>
      </CModal>
      
      {/* User Details Modal */}
      <CModal visible={showUserModal} onClose={() => setShowUserModal(false)} size="xl">
        <CModalHeader>
          <CModalTitle>
            <FontAwesomeIcon icon={faUser} className="me-2" />
            User Details: {selectedUser?.name}
          </CModalTitle>
        </CModalHeader>
        <CModalBody>
          {selectedUser && (
            <CRow>
              <CCol md={4}>
                <h6 className="text-primary border-bottom pb-2 mb-3">
                  <FontAwesomeIcon icon={faUser} className="me-2" />
                  Basic Information
                </h6>
                <div className="mb-3">
                  <strong>Name:</strong> {selectedUser.name}
                </div>
                <div className="mb-3">
                  <strong>Email:</strong> {selectedUser.email}
                </div>
                <div className="mb-3">
                  <strong>Employee ID:</strong> {selectedUser.employeeId}
                </div>
                <div className="mb-3">
                  <strong>Department:</strong> {selectedUser.department}
                </div>
                <div className="mb-3">
                  <strong>Position:</strong> {selectedUser.position}
                </div>
                <div className="mb-3">
                  <strong>Status:</strong> {getStatusBadge(selectedUser.status, isUserLocked(selectedUser))}
                </div>
                <div className="mb-3">
                  <strong>Account Active:</strong> 
                  <CBadge color={selectedUser.isActive ? 'success' : 'secondary'} className="ms-2">
                    {selectedUser.isActive ? 'Yes' : 'No'}
                  </CBadge>
                </div>
              </CCol>

              <CCol md={4}>
                <h6 className="text-primary border-bottom pb-2 mb-3">
                  <FontAwesomeIcon icon={faBuilding} className="me-2" />
                  Employment & HSSE Details
                </h6>
                <div className="mb-3">
                  <strong>Phone Number:</strong> 
                  <span className="ms-2">{selectedUser.phoneNumber || 'Not provided'}</span>
                </div>
                <div className="mb-3">
                  <strong>Work Location:</strong> 
                  <span className="ms-2">{selectedUser.workLocation || 'Not specified'}</span>
                </div>
                <div className="mb-3">
                  <strong>Cost Center:</strong> 
                  <span className="ms-2">{selectedUser.costCenter || 'Not assigned'}</span>
                </div>
                <div className="mb-3">
                  <strong>Hire Date:</strong> 
                  <span className="ms-2">{selectedUser.hireDate ? formatDate(selectedUser.hireDate) : 'Not recorded'}</span>
                </div>
                <div className="mb-3">
                  <strong>Supervisor ID:</strong> 
                  <span className="ms-2">{selectedUser.supervisorEmployeeId || 'Not assigned'}</span>
                </div>

                <h6 className="text-primary border-bottom pb-2 mb-3 mt-4">
                  <FontAwesomeIcon icon={faPhone} className="me-2" />
                  Emergency Contact
                </h6>
                <div className="mb-3">
                  <strong>Contact Name:</strong> 
                  <span className="ms-2">{selectedUser.emergencyContactName || 'Not provided'}</span>
                </div>
                <div className="mb-3">
                  <strong>Contact Phone:</strong> 
                  <span className="ms-2">{selectedUser.emergencyContactPhone || 'Not provided'}</span>
                </div>
              </CCol>

              <CCol md={4}>
                <h6 className="text-primary border-bottom pb-2 mb-3">
                  <FontAwesomeIcon icon={faShieldAlt} className="me-2" />
                  Security & Preferences
                </h6>
                <div className="mb-3">
                  <strong>MFA Required:</strong> 
                  <CBadge color={selectedUser.requiresMFA ? 'success' : 'secondary'} className="ms-2">
                    {selectedUser.requiresMFA ? 'Yes' : 'No'}
                  </CBadge>
                </div>
                <div className="mb-3">
                  <strong>Failed Login Attempts:</strong> 
                  <CBadge color={selectedUser.failedLoginAttempts > 0 ? 'warning' : 'success'} className="ms-2">
                    {selectedUser.failedLoginAttempts}
                  </CBadge>
                </div>
                <div className="mb-3">
                  <strong>Account Locked:</strong> 
                  <CBadge color={isUserLocked(selectedUser) ? 'danger' : 'success'} className="ms-2">
                    {isUserLocked(selectedUser) ? 'Yes' : 'No'}
                  </CBadge>
                </div>
                {selectedUser.accountLockedUntil && isUserLocked(selectedUser) && (
                  <div className="mb-3 small text-muted">
                    <strong>Locked Until:</strong> {formatDateTime(selectedUser.accountLockedUntil)}
                  </div>
                )}
                <div className="mb-3">
                  <strong>Preferred Language:</strong> 
                  <span className="ms-2">{selectedUser.preferredLanguage === 'id' ? 'Indonesian' : 'English'}</span>
                </div>
                <div className="mb-3">
                  <strong>Time Zone:</strong> 
                  <span className="ms-2">{selectedUser.timeZone || 'Not set'}</span>
                </div>

                <h6 className="text-primary border-bottom pb-2 mb-3 mt-4">
                  <FontAwesomeIcon icon={faHistory} className="me-2" />
                  Activity Information
                </h6>
                <div className="mb-3">
                  <strong>Last Login:</strong> 
                  <span className="ms-2">{selectedUser.lastLoginAt ? formatDateTime(selectedUser.lastLoginAt) : 'Never'}</span>
                </div>
                <div className="mb-3">
                  <strong>Password Changed:</strong> 
                  <span className="ms-2">{selectedUser.lastPasswordChange ? formatDateTime(selectedUser.lastPasswordChange) : 'Never'}</span>
                </div>
              </CCol>

              <CCol md={12}>
                <h6 className="text-primary border-bottom pb-2 mb-3 mt-4">
                  <FontAwesomeIcon icon={faUserTie} className="me-2" />
                  Assigned Roles
                </h6>
                <div className="d-flex flex-wrap gap-2 mb-3">
                  {selectedUser.roles.length > 0 ? (
                    selectedUser.roles.map((role) => (
                      <div key={role.roleId} className="d-flex align-items-center">
                        <CBadge color={getRoleBadgeColor(role.roleType)} className="me-2 px-3 py-2">
                          <strong>{role.roleName}</strong>
                        </CBadge>
                        <small className="text-muted">
                          {role.description} | Assigned: {formatDate(role.assignedAt)}
                        </small>
                      </div>
                    ))
                  ) : (
                    <span className="text-muted">No roles assigned</span>
                  )}
                </div>
              </CCol>

              <CCol md={12}>
                <h6 className="text-primary border-bottom pb-2 mb-3 mt-4">
                  <FontAwesomeIcon icon={faCalendar} className="me-2" />
                  Audit Information
                </h6>
                <CRow>
                  <CCol md={6}>
                    <div className="mb-2">
                      <strong>Created:</strong> 
                      <span className="ms-2">{formatDateTime(selectedUser.createdAt)}</span>
                    </div>
                    <div className="mb-2">
                      <strong>Created By:</strong> 
                      <span className="ms-2">{selectedUser.createdBy || 'System'}</span>
                    </div>
                  </CCol>
                  <CCol md={6}>
                    {selectedUser.lastModifiedAt && (
                      <>
                        <div className="mb-2">
                          <strong>Last Modified:</strong> 
                          <span className="ms-2">{formatDateTime(selectedUser.lastModifiedAt)}</span>
                        </div>
                        <div className="mb-2">
                          <strong>Modified By:</strong> 
                          <span className="ms-2">{selectedUser.lastModifiedBy || 'System'}</span>
                        </div>
                      </>
                    )}
                  </CCol>
                </CRow>
              </CCol>
            </CRow>
          )}
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setShowUserModal(false)}>
            Close
          </CButton>
          <CButton 
            color="primary" 
            onClick={async () => {
              setShowUserModal(false);
              if (selectedUser) {
                await handleEditUser(selectedUser);
              }
            }}
          >
            Edit User
          </CButton>
        </CModalFooter>
      </CModal>
      
      {/* Toast notifications */}
      <CToaster className="p-3" placement="top-end">
        {toasts.map((toast) => (
          <CToast key={toast.id} visible={true} color={toast.color}>
            <CToastHeader closeButton>
              <strong className="me-auto">Notification</strong>
            </CToastHeader>
            <CToastBody>{toast.message}</CToastBody>
          </CToast>
        ))}
      </CToaster>
    </CRow>
  );
};

export default UserManagement;
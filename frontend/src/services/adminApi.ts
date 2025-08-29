import { api } from './api';

export interface User {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
  lockoutEnd: string | null;
  isLocked: boolean;
}

export interface SystemMetrics {
  totalUsers: number;
  totalResumes: number;
  last24hResumes: number;
}

export interface ResumeSummary {
  resumeId: number;
  title: string;
  templateStyle: string;
  createdAt: string;
  updatedAt: string;
  personalInfo: string;
  education: string;
  experience: string;
  skills: string;
  owner: {
    id: string;
    email: string;
    fullName: string;
  };
}

export interface ResumeListResponse {
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  items: ResumeSummary[];
}

export interface ResumeDetail {
  resumeId: number;
  title: string;
  personalInfo: string;
  education: string;
  experience: string;
  skills: string;
  templateStyle: string;
  createdAt: string;
  updatedAt: string;
  aiSuggestionsJson: string | null;
  owner: {
    id: string;
    email: string;
    fullName: string;
  };
}

export interface ResumeStats {
  totalResumes: number;
  resumesLast24h: number;
  resumesLast7Days: number;
  resumesLast30Days: number;
  averageResumesPerDay: number;
  mostActiveUsers: Array<{
    userId: string;
    resumeCount: number;
  }>;
}

export interface TemplateStyle {
  templateStyle: string;
  count: number;
  lastUsed: string;
}

export interface UserActivity {
  userId: string;
  resumeCount: number;
  lastActivity: string;
  firstResume: string;
  userEmail: string;
  userName: string;
}

export const AdminApi = {
  // User management
  getUsers: () => api('/admin/users') as Promise<User[]>,
  
  addRole: (userId: string, role: string) => 
    api('/admin/users/add-role', {
      method: 'POST',
      body: JSON.stringify({ role })
    }),
  
  removeRole: (userId: string, role: string) => 
    api(`/admin/users/${userId}/roles/${role}`, {
      method: 'DELETE'
    }),
  
  changeRole: (userId: string, role: string) => 
    api(`/admin/users/${userId}/roles`, {
      method: 'POST',
      body: JSON.stringify({ role })
    }),
  
  lockUser: (userId: string) => 
    api(`/admin/users/${userId}/lock`, {
      method: 'POST',
      body: JSON.stringify({ locked: true })
    }),
  
  unlockUser: (userId: string) => 
    api(`/admin/users/${userId}/lock`, {
      method: 'POST',
      body: JSON.stringify({ locked: false })
    }),
  
  // System metrics
  getMetrics: () => api('/admin/metrics') as Promise<SystemMetrics>,
  
  // Global resume access with comprehensive filtering
  getAllResumes: (page = 1, pageSize = 20, search = '', ownerEmail = '', templateStyle = '', sortBy = 'updatedAt', sortDir = 'desc') => 
    api(`/admin/resumes?page=${page}&pageSize=${pageSize}&q=${encodeURIComponent(search)}&ownerEmail=${encodeURIComponent(ownerEmail)}&templateStyle=${encodeURIComponent(templateStyle)}&sortBy=${sortBy}&sortDir=${sortDir}`) as Promise<ResumeListResponse>,
  
  getResume: (id: number) => api(`/admin/resumes/${id}`) as Promise<ResumeDetail>,
  
  // Resume statistics and analytics
  getResumeStats: () => api('/admin/resumes/stats') as Promise<ResumeStats>,
  
  getTemplateStyles: () => api('/admin/resumes/templates') as Promise<TemplateStyle[]>,
  
  getUserActivity: () => api('/admin/users/activity') as Promise<UserActivity[]>,
};

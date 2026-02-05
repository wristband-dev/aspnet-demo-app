export type MySessionData = {
  email: string;
  hasOwnerRole: boolean;
  tenantName: string;
}

export type ApiSessionData = {
  email: string;
  tenantName: string;
  roles: {
    id: string;
    name: string;
    displayName: string;
  }[];
}

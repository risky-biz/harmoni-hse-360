# DNS Configuration

This guide explains how to configure DNS records for your Harmoni360 standalone server.

## 1. Create an A Record

1. Log in to your domain registrar's DNS management console.
2. Create an **A record** for your root domain (e.g., `your-domain.com`).
   - **Host/Name**: `@`
   - **Value**: Public IP address of your server
   - **TTL**: Use the registrar's default or `3600` seconds

## 2. Optional `www` CNAME Record

If you want `www.your-domain.com` to resolve to the same server, add a **CNAME record**:

- **Host/Name**: `www`
- **Value**: `your-domain.com`
- **TTL**: Same as above

## 3. Propagation and Verification

DNS changes may take anywhere from a few minutes to 24–48 hours to propagate globally. You can verify that the records have propagated by running:

```bash
# Check A record resolution
nslookup your-domain.com

dig your-domain.com
```

Once both commands return the correct IP address, your DNS configuration is complete.

For more details on DNS updates during migration, see [Migration Strategy](./Migration_Strategy.md).

# 🚀 SmartCenter CI/CD Deployment Guide

## 📁 **Project Structure**

### **Your Working Environment:**
- **Main Project**: `D:\webapplication_codes2022preview\SmartCenter-main\AFFZ_11012025\AFFZ_11012025.sln`
- **Git Repository**: `D:\webapplication_codes2022preview\SmartCenter-main\AFFZ_11012025\SM2025\`

### **How It Works:**
1. **Work in Main Project**: Use Visual Studio with your main project folder
2. **Deploy via Git**: Use the batch file to copy changes and trigger CI/CD
3. **Automated Deployment**: CI/CD pipeline handles all environments sequentially

## 🔄 **Sequential Deployment Process**

### **New Workflow (Triggered from main branch):**
```
Push to main branch
    ↓
1. 🔨 Build Projects (6 Projects Total)
   - AFFZ_API
   - AFFZ_Admin
   - AFFZ_Customer
   - AFFZ_Provider
   - AspireHost
   - SCAPI.ServiceDefaults
    ↓
2. 🚀 Deploy to SIT Environment
    ↓
3. 🚀 Deploy to UAT Environment (only after SIT succeeds)
    ↓
4. 🚀 Deploy to Production (only after UAT succeeds)
```

### **Benefits:**
- ✅ **Single Push**: One push to main triggers all deployments
- ✅ **Sequential**: Each stage waits for previous stage to succeed
- ✅ **Automated**: No manual intervention needed
- ✅ **Safe**: Production only deploys after UAT success
- ✅ **Complete**: All 6 projects included in deployment

## 🛠️ **How to Deploy**

### **Option 1: Using the Batch File (Recommended)**

1. **Double-click** `deploy-to-production.bat`
2. **Enter commit message** (or press Enter for default)
3. **Wait for completion** - the script will:
   - Copy all updated files from main project to Git repo
   - Commit changes with your message
   - Push to main branch
   - Trigger CI/CD pipeline

### **Option 2: Manual Git Commands**

```bash
# Navigate to Git repository
cd D:\webapplication_codes2022preview\SmartCenter-main\AFFZ_11012025\SM2025

# Copy updated files (if needed)
# ... copy your changes ...

# Add, commit, and push
git add .
git commit -m "Your commit message"
git push origin main
```

## 📋 **What Happens After You Push**

### **1. Build Stage (3-4 minutes)**
- ✅ Restore NuGet packages for all 6 projects
- ✅ Build all projects successfully
- ✅ Verify builds succeed

### **2. SIT Deployment (3-4 minutes)**
- 🚀 Publish applications to `./sit/` folder
- ✅ Create deployment manifest
- ✅ Copy environment configuration

### **3. UAT Deployment (3-4 minutes)**
- 🚀 Publish applications to `./uat/` folder
- ✅ Create deployment manifest
- ✅ Copy environment configuration

### **4. Production Deployment (3-4 minutes)**
- 🚀 Publish applications to `./production/` folder
- ✅ Create deployment manifest
- ✅ Copy environment configuration
- 🎉 **All deployments completed!**

## 🌐 **Monitoring Progress**

### **GitHub Actions:**
- **URL**: https://github.com/syedfahadiqbal7/SM2025/actions
- **Real-time updates** of deployment progress
- **Detailed logs** for each stage

### **Expected Timeline:**
- **Total Time**: 12-16 minutes
- **Build**: 3-4 minutes
- **SIT**: 3-4 minutes
- **UAT**: 3-4 minutes
- **Production**: 3-4 minutes

## ⚠️ **Important Notes**

### **File Synchronization:**
- The batch file automatically copies all project files
- **Always work in your main project folder**
- **Never edit files directly in the SM2025 folder**
- The SM2025 folder is only for Git operations

### **Environment Dependencies:**
- **UAT** only runs after **SIT** succeeds
- **Production** only runs after **UAT** succeeds
- If any stage fails, subsequent stages are skipped

### **Rollback:**
- If deployment fails, check GitHub Actions logs
- Fix the issue and push again
- Previous successful deployments remain intact

## 🚨 **Troubleshooting**

### **Common Issues:**

1. **Build Failures**
   - Check project dependencies
   - Verify .NET version compatibility
   - Review build logs in GitHub Actions

2. **Deployment Failures**
   - Check environment configurations
   - Verify folder permissions
   - Review deployment logs

3. **File Copy Issues**
   - Ensure main project path is correct
   - Check file permissions
   - Verify no files are locked by Visual Studio

### **Getting Help:**
- **GitHub Actions Logs**: Detailed error information
- **Deployment Manifests**: Status and timestamp information
- **Environment Folders**: Check published applications

## 🎯 **Best Practices**

1. **Always test locally** before deploying
2. **Use descriptive commit messages**
3. **Monitor GitHub Actions** during deployment
4. **Keep main project and Git repo in sync**
5. **Regular deployments** prevent large changes

## 📞 **Support**

If you encounter issues:
1. Check GitHub Actions logs first
2. Verify file paths and permissions
3. Ensure all projects build locally
4. Check environment configurations

---

**Happy Deploying! 🚀✨**

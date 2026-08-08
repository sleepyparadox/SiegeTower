
# Testing locally

- We will use docker to run local containers
- We will use kind to run k8s in docker
- Our `siegetower-k8s-external-admin` will create resources in main namespace `siegetower`
- Then the sigetower will manage pods within the `siegetower-workspaces`
- SiegeTrain is used for builds and release management

**Prerequisites**

```bash
docker
kind
dotnet (dotnet 10)
SiegeTrain (https://github.com/sleepyparadox/SiegeTrain)
kubectl (optional for manual debugging)
```

**Steps**


1 Ensure your local user is in docker group
```bash
groups
```
if `docker` is not visible then run
```bash
sudo usermod -aG docker "$USER"
```
Log out and log back in

2 Create a cluster

(Run as local user)

```bash
kind create cluster --name siegetower-local --config kind.siegetower-local.yaml
```


3 Verify cluster

```bash
./packages/SiegeTower.K8sExternalAdmin/dist/siegetower-k8s-external-admin config current-context
```

should return `kind-siegetower-local`

if not, you will need to switch context using kubectl

4 Push siegetower


```bash
siegetower-k8s-external-admin push
```

The app listens on `http://localhost:5006/`

(Kubernetes Service listens on NodePort `30006`)

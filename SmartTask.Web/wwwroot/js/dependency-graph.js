(function () {
    let cy = null;
    let loaded = false;

    window.initDependencyGraph = function () {
        if (loaded && cy) {
            cy.resize();
            cy.fit(undefined, 40);
            return;
        }

        const container = document.getElementById('dependencyGraph');
        const loadingEl = document.getElementById('dependencyGraphLoading');
        const emptyEl = document.getElementById('dependencyGraphEmpty');
        if (!container) return;
        const projectId = container.dataset.projectId;

        fetch(`/Dependency/GraphData?projectId=${projectId}`)
            .then(res => res.json())
            .then(data => {
                loadingEl.classList.add('d-none');

                if (!data.nodes || data.nodes.length === 0) {
                    loaded = true;
                    emptyEl.classList.remove('d-none');
                    return;
                }

                const style = getComputedStyle(document.documentElement);
                const primary = style.getPropertyValue('--primary').trim() || '#6366F1';
                const gray300 = style.getPropertyValue('--gray300').trim() || '#CBD5E1';
                const gray400 = style.getPropertyValue('--gray400').trim() || '#94A3B8';

                const elements = [];

                data.nodes.forEach(n => {
                    elements.push({
                        data: { id: 'n' + n.id, label: n.title, taskId: n.id },
                        classes: n.isAtRisk ? 'risk' : (n.isOverdue ? 'overdue' : (n.isDone ? 'done' : 'normal'))
                    });
                });

                data.edges.forEach((e, i) => {
                    elements.push({
                        data: {
                            id: 'e' + i,
                            source: 'n' + e.sourceTaskId,
                            target: 'n' + e.targetTaskId
                        },
                        classes: e.isRequired ? 'required' : 'optional'
                    });
                });

                function createGraph() {
                    try {
                        // Ensure container has real dimensions
                        if (!container.offsetWidth || !container.offsetHeight) {
                            container.style.height = '520px';
                            container.style.minHeight = '520px';
                        }

                        // Choose layout: dagre if edges exist, else grid
                        var layoutName = 'grid';
                        if (data.edges && data.edges.length > 0) {
                            layoutName = 'dagre';
                        }

                        cy = cytoscape({
                            container: container,
                            elements: elements,
                            style: [
                                {
                                    selector: 'node',
                                    style: {
                                        'label': 'data(label)',
                                        'text-valign': 'center',
                                        'text-halign': 'center',
                                        'font-size': 11,
                                        'font-weight': 700,
                                        'width': 'label',
                                        'height': 34,
                                        'padding': '10px',
                                        'shape': 'round-rectangle',
                                        'text-wrap': 'ellipsis',
                                        'text-max-width': '140px'
                                    }
                                },
                                { selector: 'node.normal', style: { 'background-color': primary, 'color': '#fff', 'border-width': 0 } },
                                { selector: 'node.overdue', style: { 'background-color': '#FFF7ED', 'color': '#C2410C', 'border-width': 2, 'border-color': '#C2410C' } },
                                { selector: 'node.risk', style: { 'background-color': '#FEE2E2', 'color': '#B91C1C', 'border-width': 2, 'border-color': '#EF4444' } },
                                { selector: 'node.done', style: { 'background-color': '#F1F5F9', 'color': '#64748B', 'border-width': 1, 'border-color': gray300 } },
                                {
                                    selector: 'edge',
                                    style: {
                                        'curve-style': 'bezier',
                                        'target-arrow-shape': 'triangle',
                                        'arrow-scale': 1,
                                        'width': 2
                                    }
                                },
                                { selector: 'edge.required', style: { 'line-color': '#EF4444', 'target-arrow-color': '#EF4444' } },
                                { selector: 'edge.optional', style: { 'line-color': gray300, 'target-arrow-color': gray300, 'line-style': 'dashed' } }
                            ],
                            layout: {
                                name: layoutName,
                                rankDir: 'RL',
                                nodeSep: 30,
                                rankSep: 70,
                                animate: false,
                                padding: 40,
                                rows: undefined
                            }
                        });

                        loaded = true;

                        // Ensure graph is visible
                        cy.resize();
                        cy.fit(undefined, 40);

                        cy.on('tap', 'node', function (evt) {
                            const taskId = evt.target.data('taskId');
                            window.location.href = `/Task/Details/${taskId}`;
                        });

                        console.log('[Graph] Rendered', cy.nodes().length, 'nodes,', cy.edges().length, 'edges, layout:', layoutName);
                    } catch (err) {
                        console.error('[Graph] Init failed:', err);
                        loaded = false;
                        emptyEl.classList.remove('d-none');
                    }
                }

                // Wait for container to have real dimensions
                function waitForDimensions(attempts) {
                    if (container.offsetWidth > 0 && container.offsetHeight > 0) {
                        createGraph();
                    } else if (attempts < 20) {
                        setTimeout(function () { waitForDimensions(attempts + 1); }, 100);
                    } else {
                        // Force dimensions and try anyway
                        container.style.height = '520px';
                        createGraph();
                    }
                }

                waitForDimensions(0);
            })
            .catch(err => {
                console.error('[Graph] Fetch failed:', err);
                loadingEl.classList.add('d-none');
                emptyEl.classList.remove('d-none');
            });

        document.getElementById('graphFitBtn').addEventListener('click', () => cy && cy.fit(undefined, 40));
        document.getElementById('graphZoomInBtn').addEventListener('click', () => cy && cy.zoom(cy.zoom() * 1.2));
        document.getElementById('graphZoomOutBtn').addEventListener('click', () => cy && cy.zoom(cy.zoom() / 1.2));
    };
})();